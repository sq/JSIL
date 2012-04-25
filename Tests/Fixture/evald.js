const serverPort = 2397;
const expirationPollInterval = 1000;
const expirationTimeout = 60000;

var http = require('http');
var url = require('url');
var fs = require('fs');
var vm = require('vm');
var console = require('console');

function RouteContext (request, response, url) {
	this.request = request;
	this.response = response;
	this.url = url;
	this.query = url.query;
	this.ended = false;
};

RouteContext.prototype.get = function (key) {
	if (key in this.query)
		return this.query[key];

	this.error("Not specified: " + key);
};

RouteContext.prototype.ok = function (text) {
	this.response.writeHead(200, {'Content-Type': 'text/plain'});
	this.end(text);
};

RouteContext.prototype.end = function (text) {
	if (this.ended)
		throw new Error("Already ended");

	this.ended = true;
	this.response.end(text);
};

RouteContext.prototype.error = function (errorText) {
	console.log("Error processing", this.request.url, ":", errorText);
	this.response.writeHead(500, {'Content-Type': 'text/plain'});
	this.end(errorText || "Unknown error");
};

RouteContext.prototype.notFound = function () {
	this.response.writeHead(404, {'Content-Type': 'text/plain'});
	this.end("File not found");
};


function addRoute (url, handler) {
	if (typeof (routes[url]) === "function")
		throw new Error("A route already exists");

	routes[url] = handler;
};

function routeRequest (request, response) {
	// Since this is set at the beginning of the request instead of the end, 
	//  long requests may cause us to timeout too early
	lastRequestHandled = Date.now();

	var requestUrl = url.parse(request.url, true);
	var key = requestUrl.pathname;

	var ctx = new RouteContext(request, response, requestUrl);

	var route = routes[key];
	if (typeof (route) === "function") {
		try {
			route(ctx);
		} catch (exc) {
			if (typeof (exc.stack) !== "undefined")
				ctx.error(String(exc.stack));
			else
				ctx.error(String(exc));
		} finally {
			if (!ctx.ended)
				ctx.ok();
		}
	} else {
		ctx.error("Unknown action: " + key);
	}
};

function checkExpiration () {
	var elapsed = Date.now() - lastRequestHandled;
	if (elapsed >= expirationTimeout) {
		console.log("Timeout expired. Exiting.");
		process.exit(0);
	}
};

function startServer () {
	var server = http.createServer(routeRequest);
	server.listen(serverPort);

	setInterval(checkExpiration, expirationPollInterval);

	console.log("Listening on port", serverPort, ", auto-terminating after", expirationTimeout, "ms");
};

function loadScript (path) {
	try {
		var scriptText = fs.readFileSync(path);
		return scriptText;
	} catch (exc) {
		console.log("Failed to load", path);
		throw exc;
	}
};

function loadGlobalScript (key, path) {
	try {
		var scriptText = loadScript(path);
		globalScripts[key] = [path, scriptText];

		vm.runInThisContext(scriptText, path);

		console.log("Loaded", path, "as", key);
	} catch (exc) {
		delete globalScripts[key];
		throw exc;
	}
};

var routes = {};
var started = Date.now(), lastRequestHandled = started;
var globalScripts = {};


var _print = function print (text) {
	var newline = "\r\n";
	console.log(this.name + ":", text);
	this.output += text + newline;
};

var _putstr = function putstr (text) {
	console.log(this.name + ":", text);
	this.output += text;
};

var _timeout = function timeout (timeoutSeconds) {
	console.log("Warning: timeout not implemented");
};

var _elapsed = function elapsed () {
	var now = Date.now();
	return (now - this.begun) / 1000;
};


addRoute("/favicon.ico", function (ctx) {
	ctx.notFound();
});

addRoute("/run", function (ctx) {
	var path = ctx.get("path");
	console.log("running", path);

	var result = {
		output: "",
		name: path,
		begun: Date.now()
	};

	print = _print.bind(result);
	putstr = _putstr.bind(result);
	timeout = _timeout.bind(result);
	elapsed = _elapsed.bind(result);

	var scriptText = loadScript(path);
	vm.runInThisContext(scriptText, path);

	console.log("ran", path);
	ctx.ok(result.output);
});

addRoute("/loadGlobal", function (ctx) {
	var key = ctx.get("key");
	var path = ctx.get("path");

	loadGlobalScript(key, path);

	ctx.ok("Loaded " + key);
});

addRoute("/reload", function (ctx) {
	var n = 0;

	for (var key in globalScripts) {
		var path = globalScripts[key][0];

		loadGlobalScript(key, path);
		n++;
	}

	ctx.ok("Reloaded " + n + " script(s)");
});

startServer();