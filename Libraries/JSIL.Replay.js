"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");


JSIL.DeclareNamespace("JSIL.Replay", false);


JSIL.Replay.Initialize = function () {
  if (jsilConfig.record)
    JSIL.Replay.InitializeRecorder();
  else if (jsilConfig.replayURI)
    JSIL.Replay.InitializePlayerFromURI(jsilConfig.replayURI);
  else if (jsilConfig.replayName)
    JSIL.Replay.InitializePlayerFromLocalStorage(jsilConfig.replayName);
};

JSIL.Replay.InitializeRecorder = function () {
  var recorder = new JSIL.Replay.Recorder();

  JSIL.Host.registerService("replayRecorder", recorder);
};

JSIL.Replay.InitializePlayerFromURI = function (uri) {
  var req = new XMLHttpRequest();
  var isDone = false;

  req.open("GET", uri, false);
  req.send();

  if ((req.status >= 200) && (req.status <= 299))
    JSIL.Replay.InitializePlayerFromJSON(req.responseText);
  else
    JSIL.Host.abort(new Error("Failed to load replay from uri '" + uri + "': " + (req.statusText || req.status)));
};

JSIL.Replay.InitializePlayerFromLocalStorage = function (name) {
  var json = localStorage[name + ".replay"];

  if (json)
    JSIL.Replay.InitializePlayerFromJSON(json);
  else
    JSIL.Host.abort(new Error("Failed to load replay '" + name + "' from localStorage."));
};

JSIL.Replay.InitializePlayerFromJSON = function (json) {
  var replay = JSON.parse(json);
  var player = new JSIL.Replay.Player(replay);

  JSIL.Host.registerService("replayPlayer", player);
};


// Recorder implementation

JSIL.Replay.Recorder = function () {
  this.serviceProxies = Object.create(null);

  this.createServiceProxies();

  this.frameScheduler = new JSIL.Replay.FrameSchedulerProxy(this, JSIL.Host.getService("frameScheduler"));
  JSIL.Host.registerService("frameScheduler", this.frameScheduler);

  this.replay = Object.create(null);
  this.replay.frameData = Object.create(null);

  for (var k in this.serviceProxies)
    this.replay.frameData[k] = [];

  this.pushFrame();
};

JSIL.Replay.Recorder.prototype.createServiceProxies = function () {
  var servicesToProxy = ["time", "keyboard", "mouse", "pageVisibility"];

  for (var i = 0, l = servicesToProxy.length; i < l; i++) {
    var key = servicesToProxy[i];
    var service = JSIL.Host.services[key];
    var proxy = new JSIL.Replay.ServiceRecordingProxy(service);

    this.serviceProxies[key] = proxy;
    JSIL.Host.registerService(key, proxy);
  }
};

JSIL.Replay.Recorder.prototype.pushFrame = function () {
  for (var key in this.serviceProxies) {
    var callList = [];
    this.serviceProxies[key].calls = callList;
    this.replay.frameData[key].push(callList);
  }
};


JSIL.Replay.RecordedCall = function (methodName, args, result, threwError) {
  this.name = methodName;
  this.result = result;
  
  if (args && args.length)
    this.args = args;

  if (threwError)
    this.threw = threwError;
};

JSIL.Replay.RecordedCall.prototype.toString = function () {
  return this.name + "(" + this.args.join(", ") + ") == " + this.result;
};


JSIL.Replay.ServiceRecordingProxy = function (service, transformArguments, transformResult) {
  this.service = service;
  this.argumentTransformer = transformArguments || function (methodName, args) { return args; };
  this.resultTransformer = transformResult || function (methodName, result) { return result; };
  this.calls = [];

  for (var k in service) {
    if (this.hasOwnProperty(k))
      continue;

    var value = service[k];
    if (typeof (value) === "function")
      this[k] = this.$makeInterceptor(k);
  }
};

JSIL.Replay.ServiceRecordingProxy.prototype.$makeInterceptor = function (name) {
  return function () {
    var args = Array.prototype.slice.call(arguments);

    var originalMethod = this.service[name];
    var failed = true;

    try {
      var result = originalMethod.apply(this.service, args);
      failed = false;

      this.calls.push(new JSIL.Replay.RecordedCall(
        name, 
        this.argumentTransformer(name, args), 
        this.resultTransformer(name, result), 
        false
      ));

      return result;
    } finally {
      if (failed)
        this.calls.push(new JSIL.Replay.RecordedCall(
          name, 
          this.argumentTransformer(name, args), 
          undefined, true
        ));
    }
  };
};


JSIL.Replay.FrameSchedulerProxy = function (recorder, service) {
  this.recorder = recorder;
  this.service = service;
  this.boundFrameCallback = this.frameCallback.bind(this);
  this.pendingFrameCallback = null;
};

JSIL.Replay.FrameSchedulerProxy.prototype.frameCallback = function () {
  var callback = this.pendingFrameCallback;
  this.pendingFrameCallback = null;

  this.recorder.pushFrame();
  callback();
};

JSIL.Replay.FrameSchedulerProxy.prototype.schedule = function (callback, when) {
  this.pendingFrameCallback = callback;
  this.service.schedule(this.boundFrameCallback, when);
};

// Player implementation

JSIL.Replay.Player = function (replay) {
  this.replay = replay;
};

JSIL.Host.initCallbacks.push(JSIL.Replay.Initialize);
