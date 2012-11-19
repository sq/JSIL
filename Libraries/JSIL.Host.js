"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.Host.isBrowser = (typeof (window) !== "undefined") && (typeof (navigator) !== "undefined");

JSIL.Host.getCanvas = function () {
  throw new Error("No canvas implementation");
};

if (typeof (console) !== "undefined") {
  try {
    JSIL.Host.logWrite = console.log.bind(console);
  } catch (e) {
    // IE :(
    JSIL.Host.logWrite = function LogWrite_IE () {
      console.log(Array.prototype.slice.call(arguments));
    }
  }
} else if (JSIL.Host.isBrowser)
  JSIL.Host.logWrite = function LogWrite_NoConsole () {};
else if (typeof (putstr) === "function")
  JSIL.Host.logWrite = putstr.bind(null);
else
  JSIL.Host.logWrite = print.bind(null);

if (typeof (console) !== "undefined") {
  try {
    JSIL.Host.logWriteLine = console.log.bind(console);
  } catch (e) {
    // IE :(
    JSIL.Host.logWriteLine = function LogWriteLine_IE () {
      console.log(Array.prototype.slice.call(arguments));
    }
  }
} else if (JSIL.Host.isBrowser)
  JSIL.Host.logWriteLine = function LogWriteLine_NoConsole () {};
else
  JSIL.Host.logWriteLine = print.bind(null);

if (typeof (console) !== "undefined") {
  try {
    JSIL.Host.warning = console.warn.bind(console);
  } catch (e) {
    // IE :(
    JSIL.Host.warning = function Warning_IE () {
      console.warn(Array.prototype.slice.call(arguments));
    }
  }
} else
  JSIL.Host.warning = JSIL.Host.logWriteLine;

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

JSIL.Host.warnedAboutRunLater = false;
JSIL.Host.pendingRunLaterItems = [];
JSIL.Host.runLaterPending = false;
JSIL.Host.runLaterCallback = function () {
  JSIL.Host.runLaterPending = false;

  var items = JSIL.Host.pendingRunLaterItems;
  var count = items.length;

  for (var i = 0; i < count; i++) {
    var item = items[i];
    item();
  }

  items.splice(0, count);
};

// This can fail to run the specified action if the host hasn't implemented it, so you should
//  only use this to run performance improvements, not things you depend on
JSIL.Host.runLater = function (action) {
  if (typeof (setTimeout) === "function") {
    JSIL.Host.pendingRunLaterItems.push(action);

    if (!JSIL.Host.runLaterPending) {
      JSIL.Host.runLaterPending = true;
      setTimeout(JSIL.Host.runLaterCallback, 0);
    }
  }
};