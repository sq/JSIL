var currentLogLine = null;

JSIL.Host.getCanvas = function (desiredWidth, desiredHeight) {
  var e = document.getElementById("canvas");
  if (!e)
    return JSIL.Host.createCanvas(desiredWidth, desiredHeight);

  if (typeof (desiredWidth) === "number")
    e.width = desiredWidth;
  if (typeof (desiredHeight) === "number")
    e.height = desiredHeight;
  
  return e;
};

JSIL.Host.createCanvas = function (desiredWidth, desiredHeight) {
  var e = document.createElement("canvas");
  e.width = desiredWidth;
  e.height = desiredHeight;
  
  return e;
};

var appendLogLine = function (elt) {
  document.getElementsByTagName("body")[0].appendChild(elt);
};

JSIL.Host.logWrite = function (text) {
  if (currentLogLine === null) {
    currentLogLine = document.createTextNode(text);
    appendLogLine(currentLogLine);
  } else {
    currentLogLine.textContent += text;
  }
};

JSIL.Host.logWriteLine = function (text) {
  var lines = text.split("\n");
  for (var i = 0, l = lines.length; i < l; i++) {
    var line = lines[i];
    if (currentLogLine === null) {
      var logLine = document.createTextNode(line);
      appendLogLine(logLine);
    } else {
      currentLogLine.textContent += line;
      currentLogLine = null;
    }
    appendLogLine(document.createElement("br"));
  }
};

var aborted = new Error("Aborted");

JSIL.Host.throwException = function (e) {
  var stack = "";
  try {
    stack = e.stack || "";
  } catch (ex) {
    stack = "";
  }

  JSIL.Host.logWriteLine("Unhandled exception: " + String(e));

  if (stack.length > 0)
    JSIL.Host.logWriteLine(stack);

  throw aborted;
};

runScript = function (text, entryPoint) {
  try {
    eval.call(this, text);

    JSIL.Initialize();

    var entryPointFunction = eval.call(this, entryPoint);

    if (typeof (entryPointFunction) !== "function") {
      throw new Error("The entry point '" + entryPoint + "' is not a function.");
    }

    entryPointFunction();
  } catch (exc) {
    if (exc !== aborted)
      JSIL.Host.throwException(exc);
  }
};