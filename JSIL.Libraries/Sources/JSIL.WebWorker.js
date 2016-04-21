"use strict";

JSIL.DeclareNamespace("JSIL.WebWorker", false);

JSIL.WebWorker.StdOutService = function () {
};

JSIL.WebWorker.StdOutService.prototype.write = function (text) {
  postMessage("output " + text);
};


JSIL.WebWorker.StdErrService = function () {
};

JSIL.WebWorker.StdErrService.prototype.write = function (text) {
  var trimmed = String(text).trim();
  if (trimmed[trimmed.length - 1] === "\n") {
    text = trimmed.substr(0, trimmed.length - 1);
  }

 postMessage("error " + text);
};

(function () {
  JSIL.Host.registerServices({
    stdout: new JSIL.WebWorker.StdOutService(),
    stderr: new JSIL.WebWorker.StdErrService()
  });
})();

function reportException (e) {
  var stack = "";
  try {
    stack = e.stack || "";
  } catch (ex) {
    stack = "";
  }

  JSIL.Host.logWriteLine("// EXCEPTION:");
  JSIL.Host.logWriteLine(String(e));
  if (stack.length > 0) {
    JSIL.Host.logWriteLine("// STACK:");
    JSIL.Host.logWriteLine(stack);
  }
  JSIL.Host.logWriteLine("// ENDEXCEPTION");

  throw e;
};

function loadAssets (assets) {
  for (var i = 0, l = assets.length; i < l; i++) {
    var assetSpec = assets[i];
  
    var assetType = assetSpec[0];
    var assetPath = assetSpec[1];
    var assetData = assetSpec[2] || null;

    var assetLoader = assetLoaders[assetType];

    assetLoader(assetPath, assetData);
  }
};
  
// onLoad will be called from the worker.
var onLoad = function  () {
  initAssetLoaders();

  var seenFilenames = {};

  var pushAsset = function (assetSpec) {
    var filename = assetSpec[1];
    if (seenFilenames[filename])
      return;

    seenFilenames[filename] = true;
    allAssetsToLoad.push(assetSpec);
  }

  var allAssetsToLoad = [];

  if (typeof (assetsToLoad) !== "undefined") {
    for (var i = 0, l = assetsToLoad.length; i < l; i++)
      pushAsset(assetsToLoad[i]);
  }

  if (typeof (contentManifest) === "object") {
    for (var k in contentManifest) {
      var subManifest = contentManifest[k];

      for (var i = 0, l = subManifest.length; i < l; i++)
        pushAsset(subManifest[i]);

    }
  }

  loadAssets(allAssetsToLoad);
    JSIL.Initialize();
    JSIL.Host.runInitCallbacks();
    if (typeof (runMain) === "function") {
        runMain();
    }
};