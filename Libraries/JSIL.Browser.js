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
JSIL.Host.getFile = function (filename) {
  var key = getShortName(filename);
  if (!allFiles.hasOwnProperty(key))
    throw new System.Exception("The file '" + key + "' is not in the asset manifest.");
  return allFiles[key];
};
JSIL.Host.getImage = function (filename) {
  var key = getShortName(filename);
  if (!allImages.hasOwnProperty(key))
    throw new System.Exception("The image '" + key + "' is not in the asset manifest.");
  return allImages[key];
};
JSIL.Host.getAsset = function (filename) {
  if (!allAssets.hasOwnProperty(filename))
    throw new System.Exception("The asset '" + filename + "' is not in the asset manifest.");
  return allAssets[filename];
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
var allImages = {};
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

function getShortName (filename) {
  var lastIndex = filename.lastIndexOf("/");
  if (lastIndex === -1)
    return filename.toLowerCase();
  
  return filename.substr(lastIndex + 1).toLowerCase();
};

function getAssetName (filename) {
  filename = getShortName(filename);
  var lastIndex = filename.lastIndexOf(".");
  if (lastIndex === -1)
    return filename;
  
  return filename.substr(0, lastIndex);
};

var assetLoaders = {
  "Script": function loadScript (filename, onError, onDoneLoading) {
    var e = document.createElement("script");
    e.type = "text/javascript";
    e.addEventListener("error", onError, true);
    e.addEventListener("load", onDoneLoading, true);
    e.id = "script_" + filename;
    e.src = filename;
    document.getElementById("scripts").appendChild(e);
  },
  "Image": function loadImage (filename, onError, onDoneLoading) {
    var e = document.createElement("img");
    e.addEventListener("error", onError, true);
    e.addEventListener("load", function () {
      allImages[getShortName(filename)] = e;
      allAssets[getAssetName(filename)] = e;
      onDoneLoading();
    }, true);
    e.id = "image_" + filename;
    e.src = filename;
    document.getElementById("images").appendChild(e);
  },
  "File": function loadBinaryFile (filename, onError, onDoneLoading) {
    var req;
    if (typeof (ActiveXObject) !== "undefined")
      req = new ActiveXObject("MSXML2.XMLHTTP");
    else
      req = new XMLHttpRequest();
    
    req.open('GET', filename, false);
    if (typeof (req.overrideMimeType) !== "undefined")
      req.overrideMimeType('text/plain; charset=x-user-defined');
            
    req.onreadystatechange = function (evt) {
      if (req.readyState != 4)
        return;
    
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
        allFiles[getShortName(filename)] = bytes;
        onDoneLoading();
      } else {
        onError(req.statusText || req.status);
      }
    };
    
    req.send(null);
  }
};

function loadNextAsset (assets, i, onDoneLoading, loadDelay) {      
  var w = (i * document.getElementById("loadingProgress").clientWidth) / (assets.length + 1);
  document.getElementById("progressBar").style.width = w.toString() + "px";
  
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
    JSIL.Host.logWriteLine("The asset '" + assetSpec + "' could not be loaded:" + String(e));
    stepCallback();
  };
  
  var assetType = assetSpec[0];
  var assetPath = assetSpec[1];
  var assetLoader = assetLoaders[assetType];
  
  if (typeof (assetLoader) !== "function") {
    errorCallback("No asset loader registered for type '" + assetType + "'.");
  } else {      
    setTimeout(function () {        
      assetLoader(assetPath, errorCallback, stepCallback);
    }, loadDelay);
  }
}

function loadAssets (assets, onDoneLoading) {
  loadNextAsset(assets, 0, onDoneLoading, 1);
}

function beginLoading () {
  document.getElementById("progressBar").style.width = "0px";
  document.getElementById("loadButton").style.display = "none";
  document.getElementById("loadingProgress").style.display = "";
  
  JSIL.Host.logWrite("Loading data ... ");
  loadAssets(assetsToLoad, function () {
    JSIL.Host.logWriteLine("done.");
    try {
      JSIL.Initialize();
      
      document.getElementById("quitButton").style.display = "";
      
      runMain();
      // Main doesn't block since we're using the browser's event loop          
    } finally {
      document.getElementById("loadingProgress").style.display = "none";
    }
  });
}

function quitGame () {
  Microsoft.Xna.Framework.Game.ForceQuit();
  document.getElementById("quitButton").style.display = "none";
}

function onLoad () {
  document.getElementById("log").value = "";
  document.getElementById("quitButton").style.display = "none";
  document.getElementById("loadingProgress").style.display = "none";
}
    
document.getElementById("quitButton").addEventListener(
  "click", quitGame, true
);
document.getElementById("loadButton").addEventListener(
  "click", beginLoading, true
);