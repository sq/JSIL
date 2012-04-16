"use strict";

var currentLogLine = null;

var webglEnabled = false;
    
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
  var slashRe = /\\/g;

  var root = JSIL.Host.getRootDirectory().toLowerCase().replace(slashRe, "/");
  var _fileRoot = fileRoot.toLowerCase().replace(slashRe, "/");
  var _filename = filename.toLowerCase().replace(slashRe, "/");

  filename = _filename.replace(root, "").replace(_fileRoot, "");
  
  while (filename[0] === "/")
    filename = filename.substr(1);
  
  return filename;
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
  var key = getAssetName(filename, false);
  if (!allAssets.hasOwnProperty(key))
    throw new System.Exception("The image '" + key + "' is not in the asset manifest.");
  return allAssets[key].image;
};
JSIL.Host.doesAssetExist = function (filename, stripRoot) {
  if (stripRoot === true) {
    var backslashRe = /\\/g;

    filename = filename.replace(backslashRe, "/").toLowerCase();
    var croot = contentRoot.replace(backslashRe, "/").toLowerCase();

    filename = filename.replace(croot, "").toLowerCase();
  }

  var key = getAssetName(filename, false);
  if (!allAssets.hasOwnProperty(key))
    return false;

  return true;
};
JSIL.Host.getAsset = function (filename, stripRoot) {
  if (stripRoot === true) {
    var backslashRe = /\\/g;

    filename = filename.replace(backslashRe, "/").toLowerCase();
    var croot = contentRoot.replace(backslashRe, "/").toLowerCase();

    filename = filename.replace(croot, "").toLowerCase();
  }

  var key = getAssetName(filename, false);
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

var $logFps = false;
var statsElement = document.getElementById("stats");

if (statsElement !== null) {
  statsElement.innerHTML = '<span title="Frames Per Second"><span id="drawsPerSecond">0</span> f/s</span><br>' +
    '<span title="Updates Per Second"><span id="updatesPerSecond">0</span> u/s</span><br>' +
    '<span title="Image Cache Size (megabytes)" id="cacheSpan"><span id="cacheSize">0.0</span >mb</span>';

  JSIL.Host.reportFps = function (drawsPerSecond, updatesPerSecond, cacheSize) {
    var e = document.getElementById("drawsPerSecond");
    e.innerHTML = drawsPerSecond.toString();
    
    e = document.getElementById("updatesPerSecond");
    e.innerHTML = updatesPerSecond.toString();
    
    if (cacheSize === "webgl") {
      e = document.getElementById("cacheSpan");
      e.className = "webgl-notice";
      e.innerHTML = "WebGL";
      e.title = "Using WebGL for rendering";
    } else {
      e = document.getElementById("cacheSize");
      e.innerHTML = (cacheSize / (1024 * 1024)).toFixed(1);
    }

    if ($logFps) {
      console.log(drawsPerSecond + " draws/s, " + updatesPerSecond + " updates/s");
    }
  };
} else {
  JSIL.Host.reportFps = function () {
    if ($logFps) {
      console.log(drawsPerSecond + " draws/s, " + updatesPerSecond + " updates/s");
    }  
  };
}

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

(function () {
  var canvas = document.getElementById("canvas");
  var originalWidth = canvas.width;
  var originalHeight = canvas.height;

  // Be a good browser citizen!
  // Disabling commonly used hotkeys makes people rage.
  var shouldIgnoreEvent = function (evt) {
    if ((document.activeElement !== null) && (document.activeElement.tagName !== "canvas"))
      return true;

    switch (evt.keyCode) {
      case 116: // F5
      case 122: // F11
        return true;
    }

    if (evt.ctrlKey) {
      switch (evt.keyCode) {
        case 67: // C
        case 78: // N
        case 84: // T
        case 86: // V
        case 88: // X
          return true;
      }
    }

    return false;
  };

  window.addEventListener(
    "keydown", function (evt) {
      if (shouldIgnoreEvent(evt))
        return;

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
      if (shouldIgnoreEvent(evt))
        return;

      evt.preventDefault();
      var keyCode = evt.keyCode;
      var codes = keyMappings[keyCode] || [keyCode];        
      
      heldKeys = heldKeys.filter(function (element, index, array) {
        return codes.indexOf(element) === -1;
      });
    }, true
  );

  canvas.addEventListener(
    "contextmenu", function (evt) {
      evt.preventDefault();
      evt.stopPropagation();
      return false;
    }, true
  );

  var mapMouseCoords = function (evt) {
      var x = evt.clientX - canvas.offsetLeft;
      var y = evt.clientY - canvas.offsetTop;

      var currentWidth = canvas.width;
      var currentHeight = canvas.height;

      x = x * originalWidth / currentWidth;
      y = y * originalHeight / currentHeight;

      mousePosition[0] = x;
      mousePosition[1] = y;
  };

  canvas.addEventListener(
    "mousedown", function (evt) {     
      mapMouseCoords(evt);

      var button = evt.button;
      if (Array.prototype.indexOf.call(heldButtons, button) === -1)
        heldButtons.push(button);

      evt.preventDefault();
      evt.stopPropagation();
      return false;
    }, true
  );

  canvas.addEventListener(
    "mouseup", function (evt) {
      mapMouseCoords(evt);
      
      var button = evt.button;
      heldButtons = heldButtons.filter(function (element, index, array) {
        (element !== button);
      });

      evt.preventDefault();
      evt.stopPropagation();
      return false;
    }, true
  );

  canvas.addEventListener(
    "mousemove", function (evt) {
      mapMouseCoords(evt);
      
      evt.preventDefault();
      evt.stopPropagation();
      return false;
    }, true
  );
})();

function getAssetName (filename, preserveCase) {
  var backslashRe = /\\/g;
  filename = filename.replace(backslashRe, "/");

  var lastIndex = filename.lastIndexOf(".");
  var result;
  if (lastIndex === -1)
    result = filename;
  else
    result = filename.substr(0, lastIndex);

  if (preserveCase === true)
    return result;
  else
    return result.toLowerCase();
};

JSIL.loadGlobalScript = function (uri, onComplete) {
  var anchor = document.createElement("a");
  anchor.href = uri;
  var absoluteUri = anchor.href;

  var scriptTag = document.createElement("script");
  scriptTag.addEventListener("load", onComplete, true);
  scriptTag.type = "text/javascript";
  scriptTag.src = absoluteUri;
  document.getElementById("scripts").appendChild(scriptTag);
};

function loadTextAsync (uri, onComplete) {
  var req;
  if ((location.protocol === "file:") && (typeof (ActiveXObject) !== "undefined")) {
    req = new ActiveXObject("MSXML2.XMLHTTP");
  } else {
    req = new XMLHttpRequest();
  }

  var state = [false];
  req.open('GET', uri, true);
          
  req.onreadystatechange = function (evt) {
    if (req.readyState != 4)
      return;

    if (state[0])
      return;
  
    state[0] = true;

    if (req.status <= 299) {
      onComplete(req.responseText, null);
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
};

function loadBinaryFileAsync (uri, onComplete) {
  var req;
  var state = [false];
  if ((location.protocol === "file:") && (typeof (ActiveXObject) !== "undefined")) {
    req = new ActiveXObject("MSXML2.XMLHTTP");
  } else {
    req = new XMLHttpRequest();
  }
  
  req.open('GET', uri, true);
  if (typeof (ArrayBuffer) === "function") {
    req.responseType = 'arraybuffer';
  }
  if (typeof (req.overrideMimeType) !== "undefined") {
    req.overrideMimeType('application/octet-stream; charset=x-user-defined');
  }
          
  req.onreadystatechange = function (evt) {
    if (req.readyState != 4)
      return;

    if (state[0])
      return;
  
    state[0] = true;
    if (req.status <= 299) {
      var bytes;
      if (
        (typeof (ArrayBuffer) === "function") &&
        (typeof (req.response) === "object")
      ) {
        var buffer = req.response;
        bytes = new Uint8Array(buffer);
      } else if (
        (typeof (req.responseBody) !== "undefined") && 
        (typeof (VBArray) !== "undefined")
      ) {
        bytes = new VBArray(req.responseBody).toArray();
      } else {
        var text = req.responseText;
        bytes = JSIL.StringToByteArray(text);
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
  "Library": function loadLibrary (filename, data, onError, onDoneLoading, state) {
    loadTextAsync(libraryRoot + filename, function (result, error) {
      var finisher = function () {
        state.pendingScriptLoads += 1;

        JSIL.loadGlobalScript(libraryRoot + filename, function () {
          state.pendingScriptLoads -= 1;
        });
      };

      if (result !== null)
        onDoneLoading(finisher);
      else
        onError(error);
    });
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    loadTextAsync(scriptRoot + filename, function (result, error) {
      var finisher = function () {
        state.pendingScriptLoads += 1;

        JSIL.loadGlobalScript(scriptRoot + filename, function () {
          state.pendingScriptLoads -= 1;
        });
      };

      if (result !== null)
        onDoneLoading(finisher);
      else
        onError(error);
    });
  },
  "Image": function loadImage (filename, data, onError, onDoneLoading) {
    var e = document.createElement("img");
    var finisher = function () {
      allAssets[getAssetName(filename)] = new HTML5ImageAsset(getAssetName(filename, true), e);
    };
    e.addEventListener("error", onError, true);
    e.addEventListener("load", onDoneLoading.bind(null, finisher), true);
    e.src = contentRoot + filename;
    document.getElementById("images").appendChild(e);
  },
  "File": function loadFile (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(fileRoot + filename, function (result, error) {
      if (result !== null) {
        allFiles[filename.toLowerCase()] = result;
        onDoneLoading(null); 
      } else {
        onError(error);
      }
    });
  },
  "SoundBank": function loadSoundBank (filename, data, onError, onDoneLoading) {
    loadTextAsync(contentRoot + filename, function (result, error) {
      if (result !== null) {
        var finisher = function () {
          allAssets[getAssetName(filename)] = JSON.parse(result);
        };
        onDoneLoading(finisher);
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

    var finisher = function () {
      allAssets[getAssetName(filename)] = new HTML5FontAsset(getAssetName(filename, true), fontId, (data || 12), e.offsetHeight);
    };
    
    var intervalHandle;

    var loadedCallback = function () {
      clearInterval(intervalHandle);      
      onDoneLoading(finisher);
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

    intervalHandle = setInterval(checkIfLoadedCallback, loadingPollInterval);
    
    // Then set up a callback to watch for the size of the element to change, and apply our CSS font.
    // In practice, the size shouldn't change until the font has loaded.
    var pointSize = (data || 12) + "pt";
    e.setAttribute("style", 'font: ' + pointSize + ' "' + fontId + '"');
  }
};

var loadWebkitSound = function (filename, data, onError, onDoneLoading) {
  var audioContext = this;
  var uri = null;

  // Safari doesn't implement canPlayType, so we just have to hard-code MP3. Lame.
  uri = contentRoot + filename + ".mp3";

  loadBinaryFileAsync(uri, function (result, error) {
    if (result !== null) {
      var buffer = audioContext.createBuffer(result.buffer, false);
      var finisher = function () {
        allAssets[getAssetName(filename)] = new WebkitSoundAsset(getAssetName(filename, true), audioContext, buffer, data);
      };
      
      onDoneLoading(finisher);
    } else {
      onError(error);
    }
  });
};

var loadHTML5Sound = function (filename, data, onError, onDoneLoading) {
  var startedLoadingWhen = Date.now();

  var e = document.createElement("audio");
  e.setAttribute("autobuffer", true);
  e.setAttribute("preload", "auto");
  
  var state = { 
    loaded: false
  };
  
  var loadingCallback = function (evt) {
    if (state.loaded)
      return;
    
    var networkState = e.networkState || 0;
    var readyState = e.readyState || 0;

    var finisher = function () {
      allAssets[getAssetName(filename)] = new HTML5SoundAsset(getAssetName(filename, true), e);
    };

    if (
      (networkState === HTMLMediaElement.NETWORK_IDLE) ||
      (networkState === HTMLMediaElement.NETWORK_LOADED /* This is in the spec, but no browser defines it? */) ||
      (readyState === HTMLMediaElement.HAVE_ENOUGH_DATA /* Chrome 12+ breaks networkState, so we have to rely on readyState */) ||
      (readyState === HTMLMediaElement.HAVE_FUTURE_DATA) ||
      (readyState === HTMLMediaElement.HAVE_CURRENT_DATA)
    ) {
      clearInterval(state.interval);
      state.loaded = true;
      onDoneLoading(finisher);
    } else if (networkState === HTMLMediaElement.NETWORK_NO_SOURCE) {
      clearInterval(state.interval);
      state.loaded = true;
      try {
        onError("Error " + e.error.code);
      } catch (ex) {
        onError("Error " + String(e.error));
      }
    }

    var now = Date.now();

    // Workaround for bug in Chrome 12+ where a load stalls indefinitely unless you spam the load method.
    if ((now - startedLoadingWhen) >= soundLoadTimeout) {
      JSIL.Host.logWriteLine("A sound file is taking forever to load. Google Chrome 12 and 13 both have a bug that can cause this, so if you're using them... try another browser.");

      clearInterval(state.interval);
      state.loaded = true;
      onDoneLoading(finisher);
    }
  };

  if (!JSIL.IsArray(data.formats)) {
    onError("Sound in manifest without any formats");
    return;
  }
  
  for (var i = 0; i < data.formats.length; i++) {
    var format = data.formats[i];
    var extension, mimetype = null;
    if (typeof (format) === "string")
      extension = format;
    else {
      extension = format.extension;
      mimetype = format.mimetype;
    }

    if (mimetype === null) {
      switch (extension) {
        case ".mp3":
          mimetype = "audio/mpeg"
          break;
        case ".ogg":
          mimetype = "audio/ogg; codecs=vorbis"
          break;
      }
    }
    
    var source = document.createElement("source");

    if (mimetype !== null)
      source.setAttribute("type", mimetype);

    source.setAttribute("src", contentRoot + filename + extension);

    e.appendChild(source);
  }
  
  document.getElementById("sounds").appendChild(e);
  
  // Events on <audio> elements are inconsistent at best across browsers, so we poll instead. :/    

  if (typeof (e.load) === "function")
    e.load();    
  state.interval = setInterval(loadingCallback, loadingPollInterval);
};

// Chrome and Safari's <audio> implementations are utter garbage.
if (typeof (webkitAudioContext) === "function") {
  assetLoaders["Sound"] = loadWebkitSound.bind(new webkitAudioContext());
} else {
  assetLoaders["Sound"] = loadHTML5Sound;
}

var $makeXNBAssetLoader = function (key, typeName) {
  assetLoaders[key] = function (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(contentRoot + filename, function (result, error) {
      if (result !== null) {
        var finisher = function () {
          var key = getAssetName(filename, false);
          var assetName = getAssetName(filename, true);
          var parsedTypeName = JSIL.ParseTypeName(typeName);    
          var type = JSIL.GetTypeInternal(parsedTypeName, JSIL.GlobalNamespace, true);
          allAssets[key] = JSIL.CreateInstanceOfType(type, [assetName, result]);
        };
        onDoneLoading(finisher); 
      } else {
        onError(error);
      }
    });
  };
};

$makeXNBAssetLoader("XNB", "RawXNBAsset");
$makeXNBAssetLoader("SpriteFont", "SpriteFontAsset");
$makeXNBAssetLoader("Texture2D", "Texture2DAsset");

function updateProgressBar (prefix, suffix, bytesLoaded, bytesTotal) {
  var loadingProgress = document.getElementById("loadingProgress");
  var progressBar = document.getElementById("progressBar");
  var progressText = document.getElementById("progressText");
  
  var w = 0;
  if (loadingProgress) {
    w = (bytesLoaded * loadingProgress.clientWidth) / (bytesTotal);
    if (w < 0)
      w = 0;
    else if (w > loadingProgress.clientWidth)
      w = loadingProgress.clientWidth;
  }

  if (progressBar)
    progressBar.style.width = w.toString() + "px";

  if (progressText) {
    progressText.innerHTML = prefix + Math.floor(bytesLoaded) + suffix + " / " + Math.floor(bytesTotal) + suffix;

    progressText.style.left = ((loadingProgress.clientWidth - progressText.clientWidth) / 2).toString() + "px";
    progressText.style.top = ((loadingProgress.clientHeight - progressText.clientHeight) / 2).toString() + "px";
  }
};

function finishLoading () {
  var state = this;

  updateProgressBar("Loading data: ", "", state.assetsFinished, state.assetCount);

  var started = Date.now();
  var endBy = started + 5;

  while (Date.now() <= endBy) {
    if (state.pendingScriptLoads > 0)
      return;

    if (state.finishIndex < state.finishQueue.length) {
      try {
        var item = state.finishQueue[state.finishIndex];
        var cb = item[2];

        if (typeof (cb) === "function")
          cb(state);
      } finally {
        state.finishIndex += 1;
        state.assetsFinished += 1;
      }
    } else {
      window.clearInterval(state.interval);
      state.interval = null;
      window.setTimeout(state.onDoneLoading, 10);
      return;
    }
  }
};

function pollAssetQueue () {      
  var state = this;

  var w = 0;
  updateProgressBar("Downloading files: ", "kb", state.bytesLoaded / 1024, state.assetBytes / 1024);

  var makeStepCallback = function (state, type, sizeBytes, i) {
    return function (finish) {
      if (typeof (finish) === "function")
        state.finishQueue.push([type, i, finish]);

      state.assetsLoading -= 1;
      state.assetsLoaded += 1;

      state.bytesLoaded += sizeBytes;
    };
  };

  var makeErrorCallback = function (assetPath, assetSpec) {
    return function (e) {
      state.assetsLoading -= 1;
      state.assetsLoaded += 1;

      allAssets[getAssetName(assetPath)] = null;

      var errorText;
      try {
        errorText = e.statusText;
      } catch (exc) {
        errorText = String(e);
      }

      JSIL.Host.logWriteLine("The asset '" + assetSpec + "' could not be loaded:" + errorText);
    };    
  };

  while ((state.assetsLoading < state.maxAssetsLoading) && (state.loadIndex < state.assetCount)) {
    try {
      var assetSpec = state.assets[state.loadIndex];
    
      var assetType = assetSpec[0];
      var assetPath = assetSpec[1];
      var assetData = assetSpec[2] || null;
      var assetLoader = assetLoaders[assetType];

      var sizeBytes = 1;
      if (assetData !== null)
        sizeBytes = assetData.sizeBytes || 1;

      var stepCallback = makeStepCallback(state, assetType, sizeBytes, state.loadIndex); 
      var errorCallback = makeErrorCallback(assetPath, assetSpec);    
      
      if (typeof (assetLoader) !== "function") {
        errorCallback("No asset loader registered for type '" + assetType + "'.");
      } else {
        state.assetsLoading += 1;
        assetLoader(assetPath, assetData, errorCallback, stepCallback, state);
      }
    } finally {
      state.loadIndex += 1;
    }
  }
    
  if (state.assetsLoaded >= state.assetCount) {
    window.clearInterval(state.interval);
    state.interval = null;

    var cmp = function (lhs, rhs) {
      if (lhs > rhs)
        return 1;
      else if (rhs > lhs)
        return -1;
      else
        return 0;
    };

    state.finishQueue.sort(function (lhs, rhs) {
      var lhsTypeIndex = 2, rhsTypeIndex = 2;
      var lhsIndex = lhs[1];
      var rhsIndex = rhs[1];

      switch (lhs[0]) {
        case "Library":
          lhsTypeIndex = 0;
          break;
        case "Script":
          lhsTypeIndex = 1;
          break;
      }

      switch (rhs[0]) {
        case "Library":
          rhsTypeIndex = 0;
          break;
        case "Script":
          rhsTypeIndex = 1;
          break;
      }

      var result = cmp(lhsTypeIndex, rhsTypeIndex);
      if (result === 0)
        result = cmp(lhsIndex, rhsIndex);

      return result;
    });

    state.interval = window.setInterval(finishLoading.bind(state), 1);

    return;
  }
};

function loadAssets (assets, onDoneLoading) {
  var state = {
    assetBytes: 0,
    assetCount: assets.length,
    bytesLoaded: 0,
    assetsLoaded: 0,
    assetsFinished: 0,
    assetsLoading: 0,
    maxAssetsLoading: 16,
    onDoneLoading: onDoneLoading,
    assets: assets,
    interval: null,
    finishQueue: [],
    loadIndex: 0,
    finishIndex: 0,
    pendingScriptLoads: 0
  };

  for (var i = 0, l = assets.length; i < l; i++) {
    var properties = assets[i][2];

    if (typeof (properties) !== "object") {
      state.assetBytes += 1;
      continue;
    }

    var sizeBytes = properties.sizeBytes || 1;
    state.assetBytes += sizeBytes;
  }

  state.interval = window.setInterval(pollAssetQueue.bind(state), 1);
};

function beginLoading () {
  var progressBar = document.getElementById("progressBar");
  var loadButton = document.getElementById("loadButton");
  var fullscreenButton = document.getElementById("fullscreenButton");
  var quitButton = document.getElementById("quitButton");
  var loadingProgress = document.getElementById("loadingProgress");
  var stats = document.getElementById("stats");
  
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

      if (fullscreenButton && canGoFullscreen)
        fullscreenButton.style.display = "";

      if (stats)
        stats.style.display = "";
      
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

var canGoFullscreen = false;

function onLoad () {
  var log = document.getElementById("log");
  var loadButton = document.getElementById("loadButton");
  var quitButton = document.getElementById("quitButton");
  var loadingProgress = document.getElementById("loadingProgress");
  var fullscreenButton = document.getElementById("fullscreenButton");
  var statsElement = document.getElementById("stats");

  if (log)
    log.value = "";
  
  if (quitButton) {
    quitButton.style.display = "none";
    quitButton.addEventListener(
      "click", quitGame, true
    );
  }

  if (statsElement)
    statsElement.style.display = "none";

  if (fullscreenButton) {
    fullscreenButton.style.display = "none";

    var canvas = document.getElementById("canvas");
    var originalWidth = canvas.width;
    var originalHeight = canvas.height;

    var reqFullscreen = canvas.requestFullScreenWithKeys || 
      canvas.mozRequestFullScreenWithKeys ||
      canvas.webkitRequestFullScreenWithKeys ||
      canvas.requestFullscreen || 
      canvas.mozRequestFullScreen || 
      canvas.webkitRequestFullScreen;

    if (reqFullscreen) {
      canGoFullscreen = true;

      var onFullscreenChange = function () {
        var isFullscreen = document.fullscreen || 
          document.mozFullScreen || 
          document.webkitIsFullScreen;

        if (isFullscreen) {
          canvas.width = screen.width;
          canvas.height = screen.height;
        } else {
          canvas.width = originalWidth;
          canvas.height = originalHeight;
        }
      };

      document.addEventListener("fullscreenchange", onFullscreenChange, false);
      document.addEventListener("mozfullscreenchange", onFullscreenChange, false);
      document.addEventListener("webkitfullscreenchange", onFullscreenChange, false);

      fullscreenButton.addEventListener(
        "click", function () {
          reqFullscreen.call(canvas, Element.ALLOW_KEYBOARD_INPUT);
        }, true
      );
    }
  };
  
  if (loadButton) {
    loadButton.addEventListener(
      "click", beginLoading, true
    );
  
    if (loadingProgress)
      loadingProgress.style.display = "none";
  } else {
    beginLoading();
  }
}
