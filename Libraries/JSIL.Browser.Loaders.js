JSIL.loadGlobalScript = function (uri, onComplete) {
  var anchor = document.createElement("a");
  anchor.href = uri;
  var absoluteUri = anchor.href;

  var body = document.getElementsByTagName("body")[0];

  var scriptTag = document.createElement("script");
  scriptTag.addEventListener("load", onComplete, true);
  scriptTag.addEventListener("error", function (e) {
    onComplete(null, e);
  }, true);
  scriptTag.type = "text/javascript";
  scriptTag.src = absoluteUri;
  body.appendChild(scriptTag);
};

var warnedAboutCORS = false;

function doXHR (uri, asBinary, onComplete) {
  var req = null, isXDR = false;

  var needCORS = jsilConfig.CORS;
  var urlPrefix = window.location.protocol + "//" + window.location.host + "/";

  var temp = document.createElement("a");
  temp.href = uri;
  var absoluteUrl = temp.href;
  temp = null;

  var sameHost = (absoluteUrl.indexOf(urlPrefix) >= 0);

  needCORS = needCORS && !sameHost;

  if ((location.protocol === "file:") && (typeof (ActiveXObject) !== "undefined")) {
    req = new ActiveXObject("MSXML2.XMLHTTP");
  } else {
    req = new XMLHttpRequest();

    if (needCORS && !("withCredentials" in req)) {
      if ((!asBinary) && (typeof (XDomainRequest) !== "undefined")) {
        isXDR = true;
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

    if (status >= 400) {
      onComplete(
        {
          response: response,
          status: status,
          statusText: statusText
        }, 
        statusText || status
      );
    } else {
      onComplete(
        {
          response: response,
          status: status,
          statusText: statusText
        }, null
      );
    }
  };
  var failed = function (error) {
    if (isDone)
      return;

    isDone = true;
    onComplete(null, error);
  };

  if (isXDR) {
    // http://social.msdn.microsoft.com/Forums/en-US/iewebdevelopment/thread/30ef3add-767c-4436-b8a9-f1ca19b4812e
    req.onprogress = function () {};

    req.onload = function () {
      succeeded(req.responseText);
    };

    req.onerror = function () {
      failed("Unknown error");
    };

    req.ontimeout = function () {
      failed("Timed out");
    };
  } else {
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
  }

  try {
    if (isXDR) {
      req.open('GET', uri);
    } else {
      req.open('GET', uri, true);
    }
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

  try {
    if (isXDR) {
      req.send(null);
    } else {
      req.send();
    }
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

var warnedAboutCORSImage = false;

var finishLoadingScript = function (state, path) {
  state.pendingScriptLoads += 1;

  JSIL.loadGlobalScript(path, function (result, error) {
    state.pendingScriptLoads -= 1;

    if (error) {
      var errorText = "Failed to load script '" + path + "'!";
      
      state.assetLoadFailures.push(
        [path, errorText]
      );

      if (jsilConfig.onLoadFailure) {
        try {
          jsilConfig.onLoadFailure(path, errorText);
        } catch (exc2) {
        }
      }
    }          
  });
};

var assetLoaders = {
  "Library": function loadLibrary (filename, data, onError, onDoneLoading, state) {
    loadTextAsync(jsilConfig.libraryRoot + filename, function (result, error) {
      var finisher = function () {
        finishLoadingScript(state, jsilConfig.libraryRoot + filename);
      };

      if ((result !== null) && (!error))
        onDoneLoading(finisher);
      else
        onError(error);
    });
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    loadTextAsync(jsilConfig.scriptRoot + filename, function (result, error) {
      var finisher = function () {
        finishLoadingScript(state, jsilConfig.scriptRoot + filename);
      };

      if ((result !== null) && (!error))
        onDoneLoading(finisher);
      else
        onError(error);
    });
  },
  "Image": function loadImage (filename, data, onError, onDoneLoading) {
    var e = document.createElement("img");
    if (jsilConfig.CORS) {
      if ("crossOrigin" in e) {
        e.crossOrigin = "";
      } else {
        if (!warnedAboutCORSImage) {
          JSIL.Host.logWriteLine("WARNING: This game requires support for CORS, and your browser does not support it for images.");
          warnedAboutCORSImage = true;
        }
      }
    }

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
      if ((result !== null) && (!error)) {
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
      if ((result !== null) && (!error)) {
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
      if ((result !== null) && (!error)) {
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
      if ((result !== null) && (!error)) {
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