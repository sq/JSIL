"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilreplay = JSIL.DeclareAssembly("JSIL.Replay");

JSIL.MakeClass($jsilcore.System.Object, "ReplayRecorder", true, [], function ($) {
  $.RawMethod(false, ".ctor", function () {
  });
});
