//
// JSIL loader. Synchronously loads all core JSIL scripts, adds essential libraries to the content manifest,
//  and loads your manifest scripts.
// Load this at the top of your document (optionally after declaring a jsilConfig dict in a previous script tag).
// Asset loading (after page load) is provided by JSIL.Browser.js.
//

if (typeof (contentManifest) !== "object") { 
  contentManifest = {}; 
};
contentManifest["JSIL"] = [];

var $jsilloaderstate = {
  environment: null,
  loadFailures: []
};

(function loadJSIL (config) {

  function Environment_Browser (config) {
    var self = this;
    this.config = config;
    this.scriptIndex = 0;
    this.scriptURIs = {};

    window.$scriptLoadFailed = function (i) {
      var uri = self.scriptURIs[i];

      if (window.JSIL && window.JSIL.Host && window.JSIL.Host.logWriteLine)
        JSIL.Host.logWriteLine("JSIL.js failed to load script '" + uri + "'");
      else if (window.console && window.console.log)
        console.error("JSIL.js failed to load script '" + uri + "'");

      $jsilloaderstate.loadFailures.push([uri]);

      if (config.onLoadFailure) {
        try {
          config.onLoadFailure(uri);
        } catch (exc) {
        }
      }
    }

    contentManifest["JSIL"].push(["Library", "JSIL.Storage.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.IO.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.JSON.js"]);  
    contentManifest["JSIL"].push(["Library", "JSIL.XML.js"]);
  };

  Environment_Browser.prototype.getUserSetting = function (key) {
    key = key.toLowerCase();

    var query = window.location.search.substring(1);
    var vars = query.split('&');

    for (var i = 0; i < vars.length; i++) {
      var pair = vars[i].split('=');

      if (decodeURIComponent(pair[0]).toLowerCase() === key) {
        if (pair.length > 1)
          return decodeURIComponent(pair[1]);
        else
          return true;
      }
    }

    return false;
  };

  Environment_Browser.prototype.loadScript = function (uri) {
    if (window.console && window.console.log)
      window.console.log("Loading '" + uri + "'...");

    this.scriptIndex += 1;
    this.scriptURIs[this.scriptIndex] = uri;

    document.write(
      "<script type=\"text/javascript\" src=\"" + uri + "\" onerror=\"$scriptLoadFailed(" +
      this.scriptIndex +
      ")\"></script>"
    );
  };

  Environment_Browser.prototype.loadEnvironmentScripts = function () {
    var libraryRoot = this.config.libraryRoot;

    this.loadScript(libraryRoot + "JSIL.Browser.js");
    this.loadScript(libraryRoot + "JSIL.Browser.Audio.js");
    this.loadScript(libraryRoot + "JSIL.Browser.Loaders.js");

    if (this.config.touch)
      this.loadScript(libraryRoot + "JSIL.Browser.Touch.js");
  };


  function Environment_SpidermonkeyShell (config) {
    var self = this;
    this.config = config;

    contentManifest["JSIL"].push(["Library", "JSIL.Storage.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.IO.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.XML.js"]);
  };

  Environment_SpidermonkeyShell.prototype.getUserSetting = function (key) {
    // FIXME
    return false;
  };

  Environment_SpidermonkeyShell.prototype.loadScript = function (uri) {
    load(uri);
  };

  Environment_SpidermonkeyShell.prototype.loadEnvironmentScripts = function () {
    this.loadScript(libraryRoot + "JSIL.Shell.js");
    this.loadScript(libraryRoot + "JSIL.Shell.Loaders.js");
  };


  var environments = {
    "browser": Environment_Browser,
    "spidermonkey_shell": Environment_SpidermonkeyShell
  }

  if (!config.environment) {
    if (typeof (window) !== "undefined")
      config.environment = "browser";
    else
      throw new Error("jsilConfig.environment not set and no default available");
  }

  var environment;

  if (typeof (config.environment) === "function") {
    environment = $jsilloaderstate.environment = new (config.environment)(config);
  } else if (typeof (config.environment) === "string") {
    var environmentType = environments[config.environment];
    if (!environmentType)
      throw new Error("No environment named '" + config.environment + "' available.");

    environment = $jsilloaderstate.environment = new (environmentType)(config);
  }

  if (typeof (config.libraryRoot) === "undefined")
    config.libraryRoot = "../Libraries/";
  
  var libraryRoot = config.libraryRoot;
  var manifestRoot = (config.manifestRoot = config.manifestRoot || "");
  config.scriptRoot = config.scriptRoot || "";
  config.fileRoot = config.fileRoot || "";
  config.assetRoot = config.assetRoot || "";

  if (typeof (config.contentRoot) === "undefined")
    config.contentRoot = "Content/";

  if (typeof (config.fileVirtualRoot) === "undefined")
    config.fileVirtualRoot = config.fileRoot || "";

  if (config.printStackTrace)
    environment.loadScript(libraryRoot + "printStackTrace.js");

  if (config.webgl2d)
    environment.loadScript(libraryRoot + "webgl-2d.js");

  if (config.gamepad)
    environment.loadScript(libraryRoot + "gamepad.js");

  environment.loadScript(libraryRoot + "ES5.js");
  environment.loadScript(libraryRoot + "mersenne.js");
  environment.loadScript(libraryRoot + "JSIL.Core.js");
  environment.loadScript(libraryRoot + "JSIL.Host.js");

  environment.loadEnvironmentScripts();
  
  environment.loadScript(libraryRoot + "JSIL.Core.Types.js");
  environment.loadScript(libraryRoot + "JSIL.References.js");
  environment.loadScript(libraryRoot + "JSIL.Bootstrap.js");
  environment.loadScript(libraryRoot + "JSIL.Bootstrap.Int64.js");
  environment.loadScript(libraryRoot + "JSIL.Bootstrap.DateTime.js");
  environment.loadScript(libraryRoot + "JSIL.Bootstrap.Text.js");
  environment.loadScript(libraryRoot + "JSIL.Bootstrap.Resources.js");

  if (config.testFixture || environment.getUserSetting("testFixture"))
    environment.loadScript(libraryRoot + "JSIL.TestFixture.js");

  config.record |= Boolean(environment.getUserSetting("record"));
  config.replayURI = environment.getUserSetting("replayURI") || config.replayURI;
  config.replayName = environment.getUserSetting("replayName") || config.replayName;
  config.fastReplay = config.fastReplay || environment.getUserSetting("fastReplay") || false;
  config.autoPlay = config.autoPlay || environment.getUserSetting("autoPlay") || config.replayURI || config.replayName || false;

  if (
    config.record || 
    config.replayURI ||
    config.replayName
  ) {
    environment.loadScript(libraryRoot + "JSIL.Replay.js");
  }

  config.disableSound = config.disableSound || environment.getUserSetting("disableSound") || false;

  config.viewportScale = parseFloat((config.viewportScale || environment.getUserSetting("viewportScale") || 1.0).toString());

  config.disableFiltering = config.disableFiltering || environment.getUserSetting("disableFiltering") || false;

  config.enableFreezeAndSeal = config.enableFreezeAndSeal || environment.getUserSetting("enableFreezeAndSeal") || false;

  var manifests = config.manifests || [];

  for (var i = 0, l = manifests.length; i < l; i++)
    environment.loadScript(manifestRoot + manifests[i] + ".manifest.js");

  if (config.winForms) {
    contentManifest["JSIL"].push(["Library", "System.Drawing.js"]);
    contentManifest["JSIL"].push(["Library", "System.Windows.js"]);
  }

  if (config.xna) {
    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Core.js"]);

    switch (Number(config.xna)) {
      case 3:
        contentManifest["JSIL"].push(["Library", "JSIL.XNA3.js"]);
        break;
      case 4:
        contentManifest["JSIL"].push(["Library", "JSIL.XNA4.js"]);
        break;
      default:
        throw new Error("Unsupported XNA version");
    }

    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Content.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Graphics.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Input.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Audio.js"]);
    contentManifest["JSIL"].push(["Library", "JSIL.XNA.Storage.js"]);
  }

  if (config.readOnlyStorage)
    contentManifest["JSIL"].push(["Library", "JSIL.ReadOnlyStorage.js"]);

  if (config.localStorage)
    contentManifest["JSIL"].push(["Library", "JSIL.LocalStorage.js"]);

})(jsilConfig || {});