"use strict";

var test = window.test = Object.create(null);

test.logText = "";
test.exceptions = [];

JSIL.Host.logWrite = function (text) {
  test.logText += text;
};
JSIL.Host.logWriteLine = function (text) {
  test.logText += text + "\r\n";
};
JSIL.Host.throwException = function (exc) {
  var exceptionMessage = "Unknown error";
  var exceptionTimestamp = JSIL.Host.getTime() - (window.$jsilbrowserstate.mainRunAtTime || 0);

  try {
    exceptionMessage = String(exc);
  } catch (_exc) {
  }

  test.exceptions.push([exceptionTimestamp, exceptionMessage]);
};

test.pressKeysFor = function (keys, duration) {
  pressKeys(keys);

  window.setTimeout(function () {
    releaseKeys(keys);
  });
};