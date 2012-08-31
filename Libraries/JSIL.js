//
// JSIL loader. Synchronously loads all core JSIL scripts, adds essential libraries to the content manifest,
//  and loads your manifest scripts.
// Load this at the top of your document (optionally after declaring a jsilConfig dict in a previous script tag).
// Asset loading (after page load) is provided by JSIL.Browser.js.
//

if (typeof (contentManifest) !== "object") { 
  contentManifest = {}; 
};
contentManifest["JSIL"] = [
    ["Library", "JSIL.Storage.js"],
    ["Library", "JSIL.IO.js"],
    ["Library", "JSIL.JSON.js"],  
    ["Library", "JSIL.XML.js"]
];

var $jsilloaderstate = {
  loadFailures: []
};

(function loadJSIL (config) { 
  var scriptIndex = 0;
  var libraryRoot = (config.libraryRoot = config.libraryRoot || "../Libraries/");
  var manifestRoot = (config.manifestRoot = config.manifestRoot || "");
  config.scriptRoot = config.scriptRoot || "";
  config.fileRoot = config.fileRoot || "";
  config.assetRoot = config.assetRoot || "";
  config.contentRoot = config.contentRoot || "Content/";
  config.fileVirtualRoot = config.fileVirtualRoot || config.fileRoot || "";

  var scriptURIs = {};

  window.$scriptLoadFailed = function (i) {
    var uri = scriptURIs[i];

    if (window.JSIL && window.JSIL.Host && window.JSIL.Host.logWriteLine)
      JSIL.Host.logWriteLine("JSIL.js failed to load script '" + uri + "'");
    else if (window.console && window.console.log)
      console.error("JSIL.js failed to load script '" + uri + "'");

    $jsilloaderstate.loadFailures.push([uri]);

    if (jsilConfig.onLoadFailure) {
      try {
        jsilConfig.onLoadFailure(uri);
      } catch (exc) {
      }
    }
  }

  function loadScript (uri) {
    if (window.console && window.console.log)
      window.console.log("Loading '" + uri + "'...");

    scriptIndex += 1;
    scriptURIs[scriptIndex] = uri;

    document.write(
      "<script type=\"text/javascript\" src=\"" + uri + "\" onerror=\"$scriptLoadFailed(" +
      scriptIndex +
      ")\"></script>"
    );
  };

  if (config.printStackTrace)
    loadScript(libraryRoot + "printStackTrace.js");

  if (config.webgl2d)
    loadScript(libraryRoot + "webgl-2d.js");

  loadScript(libraryRoot + "JSIL.Core.js");
  loadScript(libraryRoot + "JSIL.Bootstrap.js");  
  loadScript(libraryRoot + "JSIL.Browser.js");
  loadScript(libraryRoot + "JSIL.Browser.Audio.js");
  loadScript(libraryRoot + "JSIL.Browser.Loaders.js");

  if (config.testFixture || (document.location.search.indexOf("testFixture") >= 0)) {
    loadScript(libraryRoot + "JSIL.TestFixture.js");
  }

  var manifests = config.manifests || [];

  for (var i = 0, l = manifests.length; i < l; i++)
    loadScript(manifestRoot + manifests[i] + ".manifest.js");

  if (config.winForms) {
    contentManifest["JSIL"].push(
      ["Library", "System.Drawing.js"]
    );
    contentManifest["JSIL"].push(
      ["Library", "System.Windows.js"]
    );
  }

  if (config.xna) {
    contentManifest["JSIL"].push(
      ["Library", "JSIL.XNACore.js"]
    );

    switch (Number(config.xna)) {
      case 3:
        contentManifest["JSIL"].push(
          ["Library", "JSIL.XNA3.js"]
        );
        break;
      case 4:
        contentManifest["JSIL"].push(
          ["Library", "JSIL.XNA4.js"]
        );
        break;
      default:
        throw new Error("Unsupported XNA version");
    }

    contentManifest["JSIL"].push(
      ["Library", "JSIL.XNAStorage.js"]
    );
  }

  if (config.readOnlyStorage) {
    contentManifest["JSIL"].push(
        ["Library", "JSIL.ReadOnlyStorage.js"]
      );
  }

  if (config.localStorage) {
    contentManifest["JSIL"].push(
          ["Library", "JSIL.LocalStorage.js"]
        );
  }

})(jsilConfig || {});