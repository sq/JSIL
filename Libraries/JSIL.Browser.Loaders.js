JSIL.loadGlobalScript = function (uri, onComplete) {
  var anchor = document.createElement("a");
  anchor.href = uri;
  var absoluteUri = anchor.href;

  var body = document.getElementsByTagName("body")[0];

  var scriptTag = document.createElement("script");
  scriptTag.addEventListener("load", onComplete, true);
  scriptTag.type = "text/javascript";
  scriptTag.src = absoluteUri;
  body.appendChild(scriptTag);
};

var warnedAboutCORS = false;

function doXHR (uri, asBinary, onComplete) {
  var req;

  if ((location.protocol === "file:") && (typeof (ActiveXObject) !== "undefined")) {
    req = new ActiveXObject("MSXML2.XMLHTTP");
  } else {
    req = new XMLHttpRequest();

    if (jsilConfig.CORS && !("withCredentials" in req)) {
      if (false && (typeof (XDomainRequest) !== "undefined")) {
        req = new XDomainRequest();
      } else {
        if (!warnedAboutCORS) {
          JSIL.Host.logWriteLine("WARNING: This game requires support for CORS, and your browser does not appear to have it. Loading may fail.");
          warnedAboutCORS = true;
        }
      }
    }
  }

  var isDone = false;
  var succeeded = function (response, status, statusText) {
    if (isDone)
      return;

    isDone = true;
    onComplete(
      {
        response: response,
        status: status,
        statusText: statusText
      }, null
    );
  };
  var failed = function (error) {
    if (isDone)
      return;

    isDone = true;
    onComplete(null, error);
  };

  try {
    req.open('GET', uri, true);
  } catch (exc) {
    failed(exc);
  }

  if (asBinary) {
    if (typeof (ArrayBuffer) === "function") {
      req.responseType = 'arraybuffer';
    }

    if (typeof (req.overrideMimeType) !== "undefined") {
      req.overrideMimeType('application/octet-stream; charset=x-user-defined');
    }
  } else {
    if (typeof (req.overrideMimeType) !== "undefined") {
      req.overrideMimeType('text/plain; charset=x-user-defined');
    }
  }

  req.onreadystatechange = function (evt) {
    if (req.readyState != 4)
      return;

    if (isDone)
      return;

    if (asBinary) {
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

      succeeded(bytes, req.status, req.statusText);
    } else {
      succeeded(req.responseText, req.status, req.statusText);
    }
  };

  try {
    req.send();
  } catch (exc) {
    failed(exc);
  }
};

function loadTextAsync (uri, onComplete) {
  return doXHR(uri, false, function (result, error) {
    if (result)
      onComplete(result.response, error);
    else
      onComplete(null, error);
  });
};

function postProcessResultNormal (bytes) {
  return bytes;
};

var warnedAboutOpera = false;

function postProcessResultOpera (bytes) {
  // Opera sniffs content types on request bodies and if they're text, converts them to 16-bit unicode :|

  if (
    (bytes[1] === 0) &&
    (bytes[3] === 0) &&
    (bytes[5] === 0) &&
    (bytes[7] === 0)
  ) {
    if (!warnedAboutOpera) {
      JSIL.Host.logWriteLine("WARNING: Your version of Opera has a bug that corrupts downloaded file data. Please update to a new version or try a better browser.");
      warnedAboutOpera = true;
    }

    var resultBytes = new Array(bytes.length / 2);
    for (var i = 0, j = 0, l = bytes.length; i < l; i += 2, j += 1) {
      resultBytes[j] = bytes[i];
    }

    return resultBytes;
  } else {
    return bytes;
  }
};

function loadBinaryFileAsync (uri, onComplete) {
  var postProcessResult = postProcessResultNormal;  
  if (window.navigator.userAgent.indexOf("Opera/") >= 0) {
    postProcessResult = postProcessResultOpera;
  }

  return doXHR(uri, true, function (result, error) {
    if (result)
      onComplete(postProcessResult(result.response), error);
    else
      onComplete(null, error);
  });
}

var assetLoaders = {
  "Library": function loadLibrary (filename, data, onError, onDoneLoading, state) {
    loadTextAsync(jsilConfig.libraryRoot + filename, function (result, error) {
      var finisher = function () {
        state.pendingScriptLoads += 1;

        JSIL.loadGlobalScript(jsilConfig.libraryRoot + filename, function () {
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
    loadTextAsync(jsilConfig.scriptRoot + filename, function (result, error) {
      var finisher = function () {
        state.pendingScriptLoads += 1;

        JSIL.loadGlobalScript(jsilConfig.scriptRoot + filename, function () {
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
    // CORS is so dumb
    e.crossOrigin = "anonymous";

    var finisher = function () {
      $jsilbrowserstate.allAssetNames.push(filename);
      allAssets[getAssetName(filename)] = new HTML5ImageAsset(getAssetName(filename, true), e);
    };
    e.addEventListener("error", onError, true);
    e.addEventListener("load", onDoneLoading.bind(null, finisher), true);
    e.src = jsilConfig.contentRoot + filename;
  },
  "File": function loadFile (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(jsilConfig.fileRoot + filename, function (result, error) {
      if (result !== null) {
        $jsilbrowserstate.allFileNames.push(filename);
        allFiles[filename.toLowerCase()] = result;
        onDoneLoading(null); 
      } else {
        onError(error);
      }
    });
  },
  "SoundBank": function loadSoundBank (filename, data, onError, onDoneLoading) {
    loadTextAsync(jsilConfig.contentRoot + filename, function (result, error) {
      if (result !== null) {
        var finisher = function () {
          $jsilbrowserstate.allAssetNames.push(filename);
          allAssets[getAssetName(filename)] = JSON.parse(result);
        };
        onDoneLoading(finisher);
      } else {
        onError(error);
      }
    });
  },
  "Resources": function loadResources (filename, data, onError, onDoneLoading) {
    loadTextAsync(jsilConfig.scriptRoot + filename, function (result, error) {
      if (result !== null) {
        var finisher = function () {
          $jsilbrowserstate.allAssetNames.push(filename);
          allAssets[getAssetName(filename)] = JSON.parse(result);
        };
        onDoneLoading(finisher);
      } else {
        onError(error);
      }
    });
  }
};

function $makeXNBAssetLoader (key, typeName) {
  assetLoaders[key] = function (filename, data, onError, onDoneLoading) {
    loadBinaryFileAsync(jsilConfig.contentRoot + filename, function (result, error) {
      if (result !== null) {
        var finisher = function () {
          $jsilbrowserstate.allAssetNames.push(filename);
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

function initAssetLoaders () {
  initSoundLoader();

  $makeXNBAssetLoader("XNB", "RawXNBAsset");
  $makeXNBAssetLoader("SpriteFont", "SpriteFontAsset");
  $makeXNBAssetLoader("Texture2D", "Texture2DAsset");
};