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

var warnedAboutOpera = false;
var warnedAboutCORS = false;
var warnedAboutCORSImage = false;
var hasCORSXhr = false, hasCORSImage = false;
var blobBuilderInfo = {
};

function getAbsoluteUrl (localUrl) {
  var temp = document.createElement("a");
  temp.href = localUrl;
  return temp.href;
}

function doXHR (uri, asBinary, onComplete) {
  var req = null, isXDR = false;

  var needCORS = jsilConfig.CORS;
  var urlPrefix = window.location.protocol + "//" + window.location.host + "/";

  var absoluteUrl = getAbsoluteUrl(uri);
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

        onComplete(null, "CORS unavailable");
        return;
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

var loadScriptInternal = function (uri, onError, onDoneLoading, state) {
  var absoluteUrl = getAbsoluteUrl(uri);

  var finisher = function () {
    finishLoadingScript(state, uri);
  };

  if (absoluteUrl.indexOf("file://") === 0) {
    // No browser properly supports XHR against file://
    onDoneLoading(finisher);
  } else {
    loadTextAsync(uri, function (result, error) {
      if ((result !== null) && (!error))
        onDoneLoading(finisher);
      else
        onError(error);
    });
  }
};

var assetLoaders = {
  "Library": function loadLibrary (filename, data, onError, onDoneLoading, state) {
    var uri = jsilConfig.libraryRoot + filename;
    loadScriptInternal(uri, onError, onDoneLoading, state);
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    var uri = jsilConfig.scriptRoot + filename;
    loadScriptInternal(uri, onError, onDoneLoading, state);
  },
  "Image": function loadImage (filename, data, onError, onDoneLoading) {
    var e = document.createElement("img");
    if (jsilConfig.CORS) {
      if (hasCORSImage) {
        e.crossOrigin = "";
      } else if (hasCORSXhr && (blobBuilderInfo.hasBlobBuilder || blobBuilderInfo.hasBlobCtor)) {
        if (!warnedAboutCORSImage) {
          JSIL.Host.logWriteLine("WARNING: This game requires support for CORS, and your browser does not support it for images. Using workaround...");
          warnedAboutCORSImage = true;
        }

        return loadImageCORSHack(filename, data, onError, onDoneLoading);
      } else {
        if (!warnedAboutCORSImage) {
          JSIL.Host.logWriteLine("WARNING: This game requires support for CORS, and your browser does not support it.");
          warnedAboutCORSImage = true;
        }

        onError("CORS unavailable");
        return;
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

function loadImageCORSHack (filename, data, onError, onDoneLoading) {
  var sourceURL = jsilConfig.contentRoot + filename;

  // FIXME: Pass mime type through from original XHR somehow?
  var mimeType = "application/octet-stream";
  var sourceURLLower = sourceURL.toLowerCase();
  if (sourceURLLower.indexOf(".png") >= 0) {
    mimeType = "image/png";
  } else if (
    (sourceURLLower.indexOf(".jpg") >= 0) ||
    (sourceURLLower.indexOf(".jpeg") >= 0)
  ) {
    mimeType = "image/jpeg";
  }

  loadBinaryFileAsync(sourceURL, function (result, error) {
    if ((result !== null) && (!error)) {
      var objectURL = null;
      try {
        objectURL = getObjectURLForBytes(result, mimeType);
      } catch (exc) {
        onError(exc);
        return;
      }

      var e = document.createElement("img");
      var finisher = function () {
        $jsilbrowserstate.allAssetNames.push(filename);
        allAssets[getAssetName(filename)] = new HTML5ImageAsset(getAssetName(filename, true), e);
      };
      e.addEventListener("error", onError, true);
      e.addEventListener("load", onDoneLoading.bind(null, finisher), true);
      e.src = objectURL;
    } else {
      onError(error);
    }
  });
};

function initCORSHack () {
  hasCORSXhr = false;
  hasCORSImage = false;

  try {
    var xhr = new XMLHttpRequest();
    hasCORSXhr = xhr && ("withCredentials" in xhr);
  } catch (exc) {
  }

  try {
    var img = document.createElement("img");
    hasCORSImage = img && ("crossOrigin" in img);
  } catch (exc) {
  }
}

function initBlobBuilder () {
  var blobBuilder = window.WebKitBlobBuilder || window.mozBlobBuilder || window.MSBlobBuilder || window.BlobBuilder;

  blobBuilderInfo.hasObjectURL = (typeof (window.URL) !== "undefined") && (typeof (window.URL.createObjectURL) === "function");
  blobBuilderInfo.hasBlobBuilder = Boolean(blobBuilder);
  blobBuilderInfo.blobBuilder = blobBuilder;
  blobBuilderInfo.hasBlobCtor = false;

  try {
    var blob = new Blob();
    blobBuilderInfo.hasBlobCtor = Boolean(blob);
  } catch (exc) {
  }

  if (navigator.userAgent.indexOf("Firefox/14.") >= 0) {
    JSIL.Host.logWriteLine("Your browser has a serious bug that affects Escape Goat. Please update to a newer version.");
    blobBuilderInfo.hasBlobBuilder = false;
    blobBuilderInfo.hasBlobCtor = false;
  }
}

function getObjectURLForBytes (bytes, mimeType) {
  if (!blobBuilderInfo.hasObjectURL)
    throw new Error("Object URLs not available");
  else if (!("Uint8Array" in window))
    throw new Error("Typed arrays not available");

  var blob = null;

  if (Object.getPrototypeOf(bytes) !== Uint8Array.prototype)
    throw new Error("bytes must be a Uint8Array");

  try {
    if (blobBuilderInfo.hasBlobCtor) {
      blob = new Blob([bytes], { type: mimeType });
    }
  } catch (exc) {
  }

  if (!blob) {
    try {
      if (blobBuilderInfo.hasBlobBuilder) {
        var bb = new blobBuilderInfo.blobBuilder();
        bb.append(bytes.buffer);
        blob = bb.getBlob(mimeType);
      }
    } catch (exc) {
    }
  }

  if (!blob)
    throw new Error("Blob API broken or not available");

  return window.URL.createObjectURL(blob);
}

function initAssetLoaders () {
  initBlobBuilder();
  initCORSHack();
  initSoundLoader();

  $makeXNBAssetLoader("XNB", "RawXNBAsset");
  $makeXNBAssetLoader("SpriteFont", "SpriteFontAsset");
  $makeXNBAssetLoader("Texture2D", "Texture2DAsset");
};