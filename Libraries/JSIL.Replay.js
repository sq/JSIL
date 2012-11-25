"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");


JSIL.DeclareNamespace("JSIL.Replay", false);


JSIL.Replay.Recorder = function () {
  this.replay = Object.create(null);
};

JSIL.Replay.Recorder.prototype.save = function () {
};

/*

JSIL.Replay.Recorder.prototype. = function () {
};

JSIL.Replay.Recorder.prototype. = function () {
};

JSIL.Replay.Recorder.prototype. = function () {
};

*/


JSIL.Replay.Player = function (replay) {
};
