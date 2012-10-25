"use strict";

JSIL.Host.logWrite = function (text) {
  putstr(text);
};
JSIL.Host.logWriteLine = function (text) {
  print(text);
};
JSIL.Host.throwException = function (e) {
  throw e;
};

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

function shellStartup () {
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

  if (typeof (runMain) === "function") {
    JSIL.Initialize();
    runMain();
  }
}