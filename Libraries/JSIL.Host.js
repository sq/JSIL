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
  return svc.getUTC();
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

JSIL.Host.warning = function (text) {
  var svc = JSIL.Host.getService("stderr");
  svc.write(text + "\n");
}

JSIL.Host.abort = function (exception, extraInfo) {
  var svc = JSIL.Host.getService("stderr");
  if (extraInfo)
    svc.write(extraInfo);

  svc.write(exception);

  if (typeof (exception.stack) !== "undefined")
    svc.write(exception.stack);

  var svc = JSIL.Host.getService("error");
  svc.error(exception);
};

JSIL.Host.assertionFailed = function (message) {
  var svc = JSIL.Host.getService("error");
  svc.error(new Error(message || "Assertion Failed"));
};


// Default service implementations that are environment-agnostic

JSIL.Host.ES5TimeService = function () {
};

JSIL.Host.ES5TimeService.prototype.getUTC = function () {
  return Date.now();
};


JSIL.Host.ThrowErrorService = function () {
};

JSIL.Host.ThrowErrorService.prototype.error = function (exception) {
  throw exception;
};


JSIL.Host.registerServices({
  time: new JSIL.Host.ES5TimeService(),
  error: new JSIL.Host.ThrowErrorService()
});