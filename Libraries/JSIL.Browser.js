"use strict";

var currentLogLine = null;
    
JSIL.Host.getCanvas = function (desiredWidth, desiredHeight) {
  var e = document.getElementById("canvas");
  if (typeof (desiredWidth) === "number")
    e.width = desiredWidth;
  if (typeof (desiredHeight) === "number")
    e.height = desiredHeight;
  
  return e;
};
JSIL.Host.createCanvas = function (desiredWidth, desiredHeight) {
  var e = document.createElement("canvas");
  e.width = desiredWidth;
  e.height = desiredHeight;
  document.getElementById("images").appendChild(e);
  
  return e;
};
JSIL.Host.logWrite = function (text) {
  if (currentLogLine === null) {
    currentLogLine = document.createTextNode(text);
    document.getElementById("log").appendChild(currentLogLine);
  } else {
    currentLogLine.textContent += text;
  }
};
JSIL.Host.logWriteLine = function (text) {
  var lines = text.split("\n");
  for (var i = 0, l = lines.length; i < l; i++) {
    var line = lines[i];
    if (currentLogLine === null) {
      var logLine = document.createTextNode(line);
      document.getElementById("log").appendChild(logLine);
    } else {
      currentLogLine.textContent += line;
      currentLogLine = null;
    }
    document.getElementById("log").appendChild(document.createElement("br"));
  }
};
JSIL.Host.translateFilename = function (filename) {
  var root = JSIL.Host.getRootDirectory();
  filename = filename.replace(root, "");
  
  while (filename[0] === "/")
    filename = filename.substr(1);
  
  return getAssetName(filename);
}
JSIL.Host.doesFileExist = function (filename) {
  return allFiles.hasOwnProperty(JSIL.Host.translateFilename(filename));
}
JSIL.Host.getFile = function (filename) {
  if (!JSIL.Host.doesFileExist(filename))
    throw new System.Exception("The file '" + filename + "' is not in the asset manifest.");
  
  return allFiles[JSIL.Host.translateFilename(filename)];
};
JSIL.Host.getImage = function (filename) {
  var key = getAssetName(filename);
  if (!allAssets.hasOwnProperty(key))
    throw new System.Exception("The image '" + key + "' is not in the asset manifest.");
  return allAssets[key].image;
};
JSIL.Host.getAsset = function (filename) {
  var key = getAssetName(filename);
  if (!allAssets.hasOwnProperty(key))
    throw new System.Exception("The asset '" + key + "' is not in the asset manifest.");
  return allAssets[key];
};
JSIL.Host.getHeldKeys = function () {
  return Array.prototype.slice.call(heldKeys);
};
JSIL.Host.getMousePosition = function () {
  return Array.prototype.slice.call(mousePosition);
};
JSIL.Host.getHeldButtons = function () {
  return Array.prototype.slice.call(heldButtons);
};
JSIL.Host.getRootDirectory = function () {
  var url = window.location.href;
  var lastSlash = url.lastIndexOf("/");
  if (lastSlash === -1)
    return url;
  else
    return url.substr(0, lastSlash);
};
JSIL.Host.throwException = function (e) {
  var stack = "";
  try {
    stack = e.stack || "";
  } catch (ex) {
    stack = "";
  }
  JSIL.Host.logWriteLine("Unhandled exception: " + String(e));
  if (stack.length > 0)
    JSIL.Host.logWriteLine(stack);
};

var allFiles = {};
var allAssets = {};
var heldKeys = [];
var heldButtons = [];
var mousePosition = [0, 0];

// Handle mismatches between dom key codes and XNA key codes
var keyMappings = {
  16: [160, 161], // Left Shift, Right Shift
  17: [162, 163], // Left Control, Right Control
  18: [164, 165] // Left Alt, Right Alt
};

window.addEventListener(
  "keydown", function (evt) {
    evt.preventDefault();
    var keyCode = evt.keyCode;
    var codes = keyMappings[keyCode] || [keyCode];        
    
    for (var i = 0; i < codes.length; i++) {
      var code = codes[i];
      if (Array.prototype.indexOf.call(heldKeys, code) === -1)
        heldKeys.push(code);
    }
  }, true
);

window.addEventListener(
  "keyup", function (evt) {
    evt.preventDefault();
    var keyCode = evt.keyCode;
    var codes = keyMappings[keyCode] || [keyCode];        
    
    heldKeys = heldKeys.filter(function (element, index, array) {
      return codes.indexOf(element) === -1;
    });
  }, true
);

window.addEventListener(
  "mousemove", function (evt) {
    var canvas = document.getElementById("canvas");
    
    mousePosition[0] = evt.clientX - canvas.offsetLeft;
    mousePosition[1] = evt.clientY - canvas.offsetTop;
  }, true
);

function getAssetName (filename) {
  var backslashRe = /\\/g;
  filename = filename.replace(backslashRe, "/");

  var lastIndex = filename.lastIndexOf(".");
  if (lastIndex === -1)
    return filename.toLowerCase();
  
  return filename.substr(0, lastIndex).toLowerCase();
};

function loadBinaryFileAsync (uri, onComplete) {
  var req;
  var state = [false];
  if ((location.protocol === "file:") && (typeof (ActiveXObject) !== "undefined")) {
    req = new ActiveXObject("MSXML2.XMLHTTP");
  } else {
    req = new XMLHttpRequest();
  }
  
  req.open('GET', uri, false);
  if (typeof (req.overrideMimeType) !== "undefined")
    req.overrideMimeType('text/plain; charset=x-user-defined');
          
  req.onreadystatechange = function (evt) {
    if (req.readyState != 4)
      return;

    if (state[0])
      return;
  
    state[0] = true;
    if (req.status <= 299) {
      var bytes;
      if (
        (typeof (req.responseBody) !== "undefined") && 
        (typeof (VBArray) !== "undefined")
      ) {
          bytes = new VBArray(req.responseBody).toArray();
      } else {
          var text = req.responseText;
          bytes = new Array(req.responseText.length);
          for (var i = 0, l = text.length; i < l; i++)
            bytes[i] = text.charCodeAt(i) & 0xFF;
      }
      onComplete(bytes, null);
    } else {
      onComplete(null, { statusText: req.statusText, status: req.status });
      return;
    }
  };
  
  try {
    req.send(null);
  } catch (exc) {
    state[0] = true;
    onComplete(null, exc);
  }
}

var loadedFontCount = 0;
var loadingPollInterval = 25;
var soundLoadTimeout = 30000;
var fontLoadTimeout = 15000;

var assetLoaders = {
  "Library": function loadLibrary (filename, data, onError, onDoneLoading) {
    var e = document.createElement("script");
    e.type = "text/javascript";
    e.addEventListener("error", onError, true);
    e.addEventListener("load", onDoneLoading, true);
    e.async = true;
    e.src = libraryRoot + filename;
    document.getElementById("scripts").appendChild(e);
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading) {
    var e = document.createElement("script");
    e.type = "text/javascript";
    e.addEventListener("error", onError, true);
    e.addEventListener("load", onDoneLoading, true);
    e.async = true;
    e.src = scriptRoot + filename;
    document.getElementById("scripts").appendChild(e);
  },
  "Image": function loadImage (filename, data, onError, onDoneLoading) {
    var e = document.createElement("img");
    e.addEventListener("error", onError, true);
    e.addEventListener("load", function () {
      allAssets[getAssetName(filename)] = new HTML5ImageAsset(getAssetName(filename), e);
      onDoneLoading();
    }, true);
    e.src = contentRoot + filename;
    document.getElementById("images").appendChild(e);
  },
  "Sound": function loadImage (filename, data, onError, onDoneLoading) {
    var startedLoadingWhen = (new Date()).getTime();

    var e = document.createElement("audio");
    e.setAttribute("autobuffer", true);
    e.setAttribute("preload", "auto");
    if ((data !== null) && data.hasOwnProperty("loop"))
      e.loop = data.loop;
    
    var state = { 
      loaded: false
    };
    
    var loadingCallback = function (evt) {
      if (state.loaded)
        return;
      
      var networkState = e.networkState || 0;
      var readyState = e.readyState || 0;

      if (
        (networkState === HTMLMediaElement.NETWORK_IDLE) ||
        (networkState === HTMLMediaElement.NETWORK_LOADED /* This is in the spec, but no browser defines it? */) ||
        (readyState === HTMLMediaElement.HAVE_ENOUGH_DATA /* Chrome 12+ breaks networkState, so we have to rely on readyState */) ||
        (readyState === HTMLMediaElement.HAVE_FUTURE_DATA) ||
        (readyState === HTMLMediaElement.HAVE_CURRENT_DATA)
      ) {
        clearInterval(state.interval);
        state.loaded = true;
        allAssets[getAssetName(filename)] = new HTML5SoundAsset(getAssetName(filename), e);
        onDoneLoading();
      } else if (networkState === HTMLMediaElement.NETWORK_NO_SOURCE) {
        clearInterval(state.interval);
        state.loaded = true;
        allAssets[getAssetName(filename)] = new HTML5SoundAsset(getAssetName(filename), null);
        try {
          onError("Error " + e.error.code);
        } catch (ex) {
          onError("Unknown error");
        }
      }

      var now = (new Date()).getTime();

      // Workaround for bug in Chrome 12+ where a load stalls indefinitely unless you spam the load method.
      if ((now - startedLoadingWhen) >= soundLoadTimeout) {
        JSIL.Host.logWriteLine("A sound file is taking forever to load. Google Chrome 12 and 13 both have a bug that can cause this, so if you're using them... try another browser.");

        clearInterval(state.interval);
        state.loaded = true;
        allAssets[getAssetName(filename)] = new HTML5SoundAsset(getAssetName(filename), e);
        onDoneLoading();
      }
    };
    
    // Events on <audio> elements are inconsistent at best across browsers, so we poll instead. :/    
    for (var i = 0; i < data.formats.length; i++) {
      var format = data.formats[i];
      var extension, mimetype = null;
      if (typeof (format) === "string")
        extension = format;
      else {
        extension = format.extension;
        mimetype = format.mimetype;
      }
      
      var source = document.createElement("source");
      source.src = contentRoot + filename + extension;
      if (mimetype !== null)
        source.type = mimetype;
      e.appendChild(source);
    }
    
    document.getElementById("sounds").appendChild(e);
    
    if (typeof (e.load) === "function")
      e.load();
    
    state.interval = setInterval(loadingCallback, loadingPollInterval);
  },
  "File": function loadFile (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(fileRoot + filename, function (result, error) {
      if (result !== null) {
        allFiles[filename] = result;
        onDoneLoading(); 
      } else {
        onError(error);
      }
    });
  },
  "PassThrough": function loadPassThrough (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(fileRoot + filename, function (result, error) {
      if (result !== null) {
        allAssets[getAssetName(filename)] = result;
        onDoneLoading(); 
      } else {
        onError(error);
      }
    });
  },
  "Font": function loadFont (filename, data, onError, onDoneLoading) {
    var fontId = "xnafont" + loadedFontCount;
    loadedFontCount += 1;
    
    var ruleText = '@font-face {\n' +
    '  font-family: "' + fontId + '";\n' +  
    '  src: url("' + contentRoot + filename + '") format("truetype");\n' +  
    '}\n';
    
    var cssRule = document.createElement("style");    
    cssRule.type = 'text/css'
    if (cssRule.styleSheet)
      cssRule.styleSheet.cssText = ruleText;
    cssRule.appendChild(document.createTextNode(ruleText));    
    document.getElementsByTagName("head")[0].appendChild(cssRule);
    
    var e = document.createElement("span");
    e.setAttribute("id", fontId);
    e.appendChild(document.createTextNode(fontId));
    
    // We create the element with a tiny font set first and record the size...
    e.setAttribute("style", 'font: 1pt sans-serif;');
    document.getElementById("fonts").appendChild(e);
    
    var originalWidth = e.offsetWidth;
    var originalHeight = e.offsetHeight;
    var startedLoadingWhen = (new Date()).getTime();
    
    var loadedCallback = function () {
      clearInterval(intervalHandle);
      allAssets[getAssetName(filename)] = new HTML5FontAsset(getAssetName(filename), fontId, (data || 12), e.offsetHeight);
      onDoneLoading();
    };
    
    var checkIfLoadedCallback = function () {    
      var currentWidth = e.offsetWidth;
      var currentHeight = e.offsetHeight;
      var now = (new Date()).getTime();
      if ((currentWidth != originalWidth) || 
          (currentHeight != originalHeight) ||
          (now - startedLoadingWhen) > fontLoadTimeout)
        loadedCallback();
    };
    var intervalHandle = setInterval(checkIfLoadedCallback, loadingPollInterval);
    
    // Then set up a callback to watch for the size of the element to change, and apply our CSS font.
    // In practice, the size shouldn't change until the font has loaded.
    var pointSize = (data || 12) + "pt";
    e.setAttribute("style", 'font: ' + pointSize + ' "' + fontId + '"');
  }
};

function loadNextAsset (assets, i, onDoneLoading, loadDelay) {      
  var w = 0;
  var loadingProgress = document.getElementById("loadingProgress");
  var progressBar = document.getElementById("progressBar");
  
  if (loadingProgress)
    w = (i * loadingProgress.clientWidth) / (assets.length + 1);
  if (progressBar)
    progressBar.style.width = w.toString() + "px";
  
  if (i >= assets.length) {
    setTimeout(onDoneLoading, loadDelay);
    return;
  }

  var assetSpec = assets[i];
  var j = i + 1;
  
  var stepCallback = function () {
    loadNextAsset(assets, j, onDoneLoading, loadDelay);
  };
  
  var errorCallback = function (e) {
    allAssets[getAssetName(assetPath)] = null;
    JSIL.Host.logWriteLine("The asset '" + assetSpec + "' could not be loaded:" + String(e));
    stepCallback();
  };
  
  var assetType = assetSpec[0];
  var assetPath = assetSpec[1];
  var assetData = assetSpec[2] || null;
  var assetLoader = assetLoaders[assetType];
  
  if (typeof (assetLoader) !== "function") {
    errorCallback("No asset loader registered for type '" + assetType + "'.");
  } else {      
    setTimeout(function () {        
      assetLoader(assetPath, assetData, errorCallback, stepCallback);
    }, loadDelay);
  }
}

function loadAssets (assets, onDoneLoading) {
  loadNextAsset(assets, 0, onDoneLoading, 1);
}

function beginLoading () {
  var progressBar = document.getElementById("progressBar");
  var loadButton = document.getElementById("loadButton");
  var quitButton = document.getElementById("quitButton");
  var loadingProgress = document.getElementById("loadingProgress");
  
  if (progressBar)
    progressBar.style.width = "0px";
  if (loadButton)
    loadButton.style.display = "none";
  if (loadingProgress)
    loadingProgress.style.display = "";

  var allAssets = [];
  for (var i = 0, l = assetsToLoad.length; i < l; i++)
    allAssets.push(assetsToLoad[i]);

  if (typeof (contentManifest) === "object") {
    for (var k in contentManifest) {
      var subManifest = contentManifest[k];

      for (var i = 0, l = subManifest.length; i < l; i++)
        allAssets.push(subManifest[i]);

    }
  }
  
  JSIL.Host.logWrite("Loading data ... ");
  loadAssets(allAssets, function () {
    JSIL.Host.logWriteLine("done.");
    try {
      JSIL.Initialize();
      
      if (quitButton)
        quitButton.style.display = "";
      
      runMain();
      // Main doesn't block since we're using the browser's event loop          
    } finally {
      if (loadingProgress)
        loadingProgress.style.display = "none";
    }
  });
}

function quitGame () {
  Microsoft.Xna.Framework.Game.ForceQuit();
  document.getElementById("quitButton").style.display = "none";
}

function onLoad () {
  var log = document.getElementById("log");
  var loadButton = document.getElementById("loadButton");
  var quitButton = document.getElementById("quitButton");
  var loadingProgress = document.getElementById("loadingProgress");
  
  if (log)
    log.value = "";
  
  if (quitButton) {
    quitButton.style.display = "none";
    quitButton.addEventListener(
      "click", quitGame, true
    );
  }
  
  if (loadButton) {
    loadButton.addEventListener(
      "click", beginLoading, true
    );
  }
  
  if (loadingProgress)
    loadingProgress.style.display = "none";
}
