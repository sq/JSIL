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


JSIL.Replay.Recorder = function () {
  this.replay = Object.create(null);
};


JSIL.Replay.Player = function (replay) {
  this.replay = replay;
};


JSIL.Host.initCallbacks.push(JSIL.Replay.Initialize);
