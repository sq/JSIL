"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.Host.isBrowser = (typeof (window) !== "undefined") && (typeof (navigator) !== "undefined");


JSIL.Host.services = Object.create(null);

JSIL.Host.getService = function (key, noThrow) {
  var svc = JSIL.Host.services[key];
  if (!svc) {
    if (noThrow)
      return null;
    else
      throw new Error("Service '" + key + "' not available");
  }

  return svc;
};

JSIL.Host.registerServices = function (services) {
  for (var key in services) {
    if (!services.hasOwnProperty(key))
      continue;

    JSIL.Host.services[key] = services[key];
  }
};


// Access services using these methods instead of getService directly.

JSIL.Host.getTime = function () {
  var svc = JSIL.Host.getService("time");
  return svc.get();
};

JSIL.Host.getCanvas = function (desiredWidth, desiredHeight) {
  var svc = JSIL.Host.getService("canvas");
  return svc.get(desiredWidth, desiredHeight);
};

JSIL.Host.createCanvas = function (desiredWidth, desiredHeight) {
  var svc = JSIL.Host.getService("canvas");
  return svc.create(desiredWidth, desiredHeight);
};

JSIL.Host.getHeldKeys = function () {
  var svc = JSIL.Host.getService("keyboard", true);
  if (!svc)
    return [];

  return svc.getHeldKeys();
};

JSIL.Host.getMousePosition = function () {
  var svc = JSIL.Host.getService("mouse", true);
  if (!svc)
    return [0, 0];

  return svc.getPosition();
};

JSIL.Host.getHeldMouseButtons = function () {
  var svc = JSIL.Host.getService("mouse", true);
  if (!svc)
    return [];

  return svc.getHeldButtons();
};

JSIL.Host.isPageVisible = function () {
  var svc = JSIL.Host.getService("pageVisibility", true);
  if (!svc)
    return true;

  return svc.get();
};

JSIL.Host.runLater = function (action) {
  var svc = JSIL.Host.getService("runLater", true);
  if (!svc)
    return false;

  svc.enqueue(action);
  return true;
};

JSIL.Host.logWrite = function (text) {
  var svc = JSIL.Host.getService("stdout");
  svc.write(text);
};

JSIL.Host.logWriteLine = function (text) {
  var svc = JSIL.Host.getService("stdout");
  svc.write(text + "\n");
};


// Default service implementations that are environment-agnostic

JSIL.Host.error = function (exception, text) {
  if (typeof (console) !== "undefined") {
    var rest = Array.prototype.slice.call(arguments, 1);
    rest.push(exception);

    var stack = null;
    try {
      stack = exception.stack;
    } catch (e) {
      stack = null;
    }

    if ((typeof (stack) !== "undefined") && (stack !== null)) {
      if (stack.indexOf(String(exception)) >= 0)
        rest.pop();

      rest.push(stack);
    }

    Function.prototype.apply.call(console.error, console, rest);
  }

  JSIL.Host.throwException(exception);
};

JSIL.Host.throwException = function (e) {
  throw e;
};

JSIL.Host.assertionFailed = function (message) {
  JSIL.Host.error(new Error(message || "Assertion Failed"));
};