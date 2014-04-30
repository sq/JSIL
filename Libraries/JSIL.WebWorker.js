JSIL.DeclareNamespace("JSIL.WebWorker", false);

var currentLogLine = null;

var webglEnabled = false;

var $jsilbrowserstate = window.$jsilbrowserstate = {
    allFileNames: [],
    allAssetNames: [],
    readOnlyStorage: null,
    heldKeys: [],
    heldButtons: [],
    mousePosition: [0, 0],
    isLoading: false,
    isLoaded: false,
    isMainRunning: false,
    hasMainRun: false,
    mainRunAtTime: 0,
    blockKeyboardInput: false,
    blockGamepadInput: false
};

JSIL.WebWorker.PageVisibilityService = function () {
};

JSIL.WebWorker.PageVisibilityService.prototype.keys = ["hidden", "mozHidden", "msHidden", "webkitHidden"];

JSIL.WebWorker.PageVisibilityService.prototype.get = function () {
    for (var i = 0, l = this.keys.length; i < l; i++) {
        var key = this.keys[i];
        var value = document[key];

        if (typeof (value) !== "undefined")
            return !value;
    }

    return true;
};


JSIL.WebWorker.RunLaterService = function () {
    this.queue = [];
    this.pending = false;
    this.boundStep = this.step.bind(this);
};

JSIL.WebWorker.RunLaterService.prototype.enqueue = function (callback) {
    this.queue.push(callback);

    if (!this.pending) {
        this.pending = true;
        window.setTimeout(this.boundStep, 0);
    }
};

JSIL.WebWorker.RunLaterService.prototype.step = function () {
    var count = this.queue.length;
    this.pending = false;

    for (var i = 0; i < count; i++) {
        var item = this.queue[i];
        item();
    }

    this.queue.splice(0, count);
};


JSIL.WebWorker.LogService = function () {
    this.currentLine = null;
};

JSIL.WebWorker.LogService.prototype.write = function (text) {
    postMessage({ operation: 'log', text: text });
};


JSIL.WebWorker.WarningService = function (stream) {
    this.stream = stream;
};

JSIL.WebWorker.WarningService.prototype.write = function (text) {
    // Quirky behavior, but we suppress warnings from the log if the console is available.
    if (console && console.warn) {
        if (typeof (text) === "string") {
            console.warn(text.trim());
        }
        else
            console.warn(text);
    } else if (this.stream) {
        this.stream.write(text);
    }
};

JSIL.WebWorker.LocalStorageService = function (storage) {
    this.storage = storage;
};

JSIL.WebWorker.LocalStorageService.prototype.getItem = function (key) {
    return this.storage.getItem(key);
};

JSIL.WebWorker.LocalStorageService.prototype.setItem = function (key, text) {
    return this.storage.setItem(key, text);
};

JSIL.WebWorker.LocalStorageService.prototype.removeItem = function (key) {
    return this.storage.removeItem(key);
};

JSIL.WebWorker.LocalStorageService.prototype.getKeys = function () {
    var result = new Array(this.storage.length);

    for (var i = 0, l = result.length; i < l; i++)
        result[i] = this.storage.key(i);

    return result;
};


JSIL.WebWorker.WindowService = function (window) {
    this.window = window;
};

JSIL.WebWorker.WindowService.prototype.alert = function () {
    return this.window.alert.apply(this.window, arguments);
};

JSIL.WebWorker.WindowService.prototype.prompt = function () {
    return this.window.prompt.apply(this.window, arguments);
};

JSIL.WebWorker.WindowService.prototype.getTitle = function () {
    return this.window.title;
};

JSIL.WebWorker.WindowService.prototype.setTitle = function (value) {
    return this.window.document.title = this.window.title = value;
};

JSIL.WebWorker.WindowService.prototype.getLocationHref = function () {
    return this.window.location.href;
};

JSIL.WebWorker.WindowService.prototype.getLocationHash = function () {
    return this.window.location.hash;
};

JSIL.WebWorker.WindowService.prototype.getLocationSearch = function () {
    return this.window.location.search;
};

JSIL.WebWorker.WindowService.prototype.getNavigatorUserAgent = function () {
    return this.window.navigator.userAgent;
};

JSIL.WebWorker.WindowService.prototype.getNavigatorLanguage = function () {
    return this.window.navigator.language ||
      this.window.navigator.userLanguage ||
      this.window.navigator.systemLanguage ||
      null;
};

JSIL.WebWorker.WindowService.prototype.getPerformanceUsedJSHeapSize = function () {
    if (
      (typeof (this.window.performance) !== "undefined") &&
      (typeof (this.window.performance.memory) !== "undefined")
    ) {
        return this.window.performance.memory.usedJSHeapSize;
    } else {
        return 0;
    }
};


JSIL.WebWorker.HistoryService = function (history) {
    this.history = history;
    this.canPushState = typeof (this.history.pushState) === "function";
};

JSIL.WebWorker.HistoryService.prototype.pushState = function (a, b, c) {
    return this.history.pushState(a, b, c);
};

JSIL.WebWorker.HistoryService.prototype.replaceState = function (a, b, c) {
    return this.history.replaceState(a, b, c);
};

JSIL.WebWorker.TraceService = function (console) {
    this.console = console;
};

JSIL.WebWorker.TraceService.prototype.write = function (text, category) {
    if (this.console) {
        if (arguments.length === 2)
            this.console.log(category + ": " + text);
        else if (arguments.length === 1)
            this.console.log(text);
    }
};

JSIL.WebWorker.TraceService.prototype.information = function (text) {
    if (this.console)
        this.console.log(text);
};

JSIL.WebWorker.TraceService.prototype.warning = function (text) {
    if (this.console)
        this.console.warn(text);
};

JSIL.WebWorker.TraceService.prototype.error = function (text) {
    if (this.console)
        this.console.error(text);
};


(function () {
    var logSvc = new JSIL.WebWorker.LogService();

    JSIL.Host.registerServices({
        pageVisibility: new JSIL.WebWorker.PageVisibilityService(),
        runLater: new JSIL.WebWorker.RunLaterService(),
        stdout: logSvc,
        stderr: new JSIL.WebWorker.WarningService(logSvc)
    });

    if (typeof (localStorage) !== "undefined")
        JSIL.Host.registerService("localStorage", new JSIL.WebWorker.LocalStorageService(localStorage));
})();


JSIL.Host.translateFilename = function (filename) {
    if (filename === null)
        return null;

    var slashRe = /\\/g;

    var root = JSIL.Host.getRootDirectory().toLowerCase().replace(slashRe, "/");
    var _fileRoot = jsilConfig.fileRoot.toLowerCase().replace(slashRe, "/");
    var _filename = filename.replace(slashRe, "/").toLowerCase();

    while (_filename[0] === "/")
        _filename = _filename.substr(1);

    if (_filename.indexOf(root) === 0)
        _filename = _filename.substr(root.length);

    while (_filename[0] === "/")
        _filename = _filename.substr(1);

    if (_filename.indexOf(_fileRoot) === 0)
        _filename = _filename.substr(_fileRoot.length);

    while (_filename[0] === "/")
        _filename = _filename.substr(1);

    return _filename;
}
JSIL.Host.getImage = function (filename) {
    var key = getAssetName(filename, false);
    if (!allAssets.hasOwnProperty(key))
        throw new System.IO.FileNotFoundException("The image '" + key + "' is not in the asset manifest.", filename);

    return allAssets[key].image;
};
JSIL.Host.doesAssetExist = function (filename, stripRoot) {
    if (filename === null)
        return false;

    if (stripRoot === true) {
        var backslashRe = /\\/g;

        filename = filename.replace(backslashRe, "/").toLowerCase();
        var croot = jsilConfig.contentRoot.replace(backslashRe, "/").toLowerCase();

        filename = filename.replace(croot, "").toLowerCase();
    }

    var key = getAssetName(filename, false);
    if (!allAssets.hasOwnProperty(key))
        return false;

    return true;
};
JSIL.Host.getAsset = function (filename, stripRoot) {
    if (filename === null)
        throw new System.Exception("Filename was null");

    if (stripRoot === true) {
        var backslashRe = /\\/g;

        filename = filename.replace(backslashRe, "/").toLowerCase();
        var croot = jsilConfig.contentRoot.replace(backslashRe, "/").toLowerCase();

        filename = filename.replace(croot, "").toLowerCase();
    }

    var key = getAssetName(filename, false);
    if (!allAssets.hasOwnProperty(key))
        throw new System.IO.FileNotFoundException("The asset '" + key + "' is not in the asset manifest.", filename);

    return allAssets[key];
};
JSIL.Host.getRootDirectory = function () {
    var url = window.location.href;
    var lastSlash = url.lastIndexOf("/");
    if (lastSlash === -1)
        return url;
    else
        return url.substr(0, lastSlash);
};
JSIL.Host.getStorageRoot = function () {
    return $jsilbrowserstate.storageRoot;
};


var $logFps = false;

var allFiles = {};
var allAssets = {};
var allManifestResources = {};

function getAssetName(filename, preserveCase) {
    var backslashRe = /\\/g;
    filename = filename.replace(backslashRe, "/");

    var doubleSlashRe = /\/\//g;
    while (filename.indexOf("//") >= 0)
        filename = filename.replace(doubleSlashRe, "/");

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

var loadedFontCount = 0;
var loadingPollInterval = 1;
var maxAssetsLoading = 4;
var soundLoadTimeout = 30000;
var fontLoadTimeout = 10000;
var finishStepDuration = 25;

function finishLoading(st) {
    var state = st;

    var started = Date.now();
    var endBy = started + finishStepDuration;

    var initFileStorage = function (volume) {
        for (var i = 0, l = $jsilbrowserstate.allFileNames.length; i < l; i++) {
            var filename = $jsilbrowserstate.allFileNames[i];
            var file = volume.createFile(filename, false, true);
            file.writeAllBytes(allFiles[filename.toLowerCase()]);
        }
    };

    var initIfNeeded = function () {
        if (!state.jsilInitialized) {
            state.jsilInitialized = true;
            JSIL.Initialize();
        }

        if (state.initFailed) {
            return;
        }

        try {
            if (typeof ($jsilreadonlystorage) !== "undefined") {
                var prefixedFileRoot;

                if (jsilConfig.fileVirtualRoot[0] !== "/")
                    prefixedFileRoot = "/" + jsilConfig.fileVirtualRoot;
                else
                    prefixedFileRoot = jsilConfig.fileVirtualRoot;

                $jsilbrowserstate.readOnlyStorage = new ReadOnlyStorageVolume("files", prefixedFileRoot, initFileStorage);
            }

            JSIL.SetLazyValueProperty($jsilbrowserstate, "storageRoot", function InitStorageRoot() {
                var root;
                if (JSIL.GetStorageVolumes) {
                    var volumes = JSIL.GetStorageVolumes();

                    if (volumes.length) {
                        root = volumes[0];
                    }
                }

                if (!root && typeof (VirtualVolume) === "function") {
                    root = new VirtualVolume("root", "/");
                }

                if (root) {
                    if ($jsilbrowserstate.readOnlyStorage) {
                        var trimmedRoot = jsilConfig.fileVirtualRoot.trim();

                        if (trimmedRoot !== "/" && trimmedRoot)
                            root.createJunction(jsilConfig.fileVirtualRoot, $jsilbrowserstate.readOnlyStorage.rootDirectory, false);
                        else
                            root = $jsilbrowserstate.readOnlyStorage;
                    }

                    return root;
                }

                return null;
            });

            state.initFailed = false;
        } catch (exc) {
            state.initFailed = true;

            throw exc;
        }
    };

    while (state.finishIndex < state.finishQueue.length) {
        try {
            var item = state.finishQueue[state.finishIndex];
            var cb = item[2];

            // Ensure that we initialize the JSIL runtime before constructing asset objects.
            if ((item[0] != "Script") && (item[0] != "Library")) {
                initIfNeeded();
            }

            if (typeof (cb) === "function") {
                cb(state);
            }
        } catch (exc) {
            state.assetLoadFailures.push(
              [item[3], exc]
            );

            if (jsilConfig.onLoadFailure) {
                try {
                    jsilConfig.onLoadFailure(item[3], exc);
                } catch (exc2) {
                }
            }
        } finally {
            state.finishIndex += 1;
            state.assetsFinished += 1;
        }
    }
    initIfNeeded();

    var allFailures = $jsilloaderstate.loadFailures.concat(state.assetLoadFailures);

    return;
};

function pollAssetQueue(st) {
    var state = st;

    var w = 0;

    var makeStepCallback = function (state, type, sizeBytes, i, name) {
        return function (finish) {
            var realName = name;

            var lastDot = name.lastIndexOf(".");
            if (lastDot >= 0)
                name = name.substr(0, lastDot);

            var firstComma = name.indexOf(",");
            if (firstComma >= 0)
                name = name.substr(0, firstComma);

            if (typeof (finish) === "function")
                state.finishQueue.push([type, i, finish, name]);

            delete state.assetsLoadingNames[realName];
            state.assetsLoading -= 1;
            state.assetsLoaded += 1;

            state.bytesLoaded += sizeBytes;
        };
    };

    var makeErrorCallback = function (assetPath, assetSpec) {
        return function (e) {
            delete state.assetsLoadingNames[getAssetName(assetPath)];
            state.assetsLoading -= 1;
            state.assetsLoaded += 1;

            allAssets[getAssetName(assetPath)] = null;

            var errorText = stringifyLoadError(e);

            state.assetLoadFailures.push(
              [assetPath, errorText]
            );

            if (jsilConfig.onLoadFailure) {
                try {
                    jsilConfig.onLoadFailure(item[3], errorText);
                } catch (exc2) {
                }
            }

            JSIL.Host.logWriteLine("The asset '" + assetPath + "' could not be loaded: " + errorText);
        };
    };

    while ((state.assetsLoading < maxAssetsLoading) && (state.loadIndex < state.assetCount)) {
        try {
            var assetSpec = state.assets[state.loadIndex];

            var assetType = assetSpec[0];
            var assetPath = assetSpec[1];
            var assetData = assetSpec[2] || null;
            var assetLoader = assetLoaders[assetType];

            var sizeBytes = 1;
            if (assetData !== null)
                sizeBytes = assetData.sizeBytes || 1;

            var stepCallback = makeStepCallback(state, assetType, sizeBytes, state.loadIndex, assetPath);
            var errorCallback = makeErrorCallback(assetPath, assetSpec);

            if (typeof (assetLoader) !== "function") {
                errorCallback("No asset loader registered for type '" + assetType + "'.");
            } else {
                state.assetsLoading += 1;
                state.assetsLoadingNames[assetPath] = assetLoader;
                assetLoader(assetPath, assetData, errorCallback, stepCallback, state);
            }
        } finally {
            state.loadIndex += 1;
        }
    }

    if (state.assetsLoaded >= state.assetCount) {
        state.assetsLoadingNames = {};

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

            var result = JSIL.CompareValues(lhsTypeIndex, rhsTypeIndex);
            if (result === 0)
                result = JSIL.CompareValues(lhsIndex, rhsIndex);

            return result;
        });

        finishLoading(state);

        return;
    }
};

function loadAssets(assets) {
    var state = {
        assetBytes: 0,
        assetCount: assets.length,
        bytesLoaded: 0,
        assetsLoaded: 0,
        assetsFinished: 0,
        assetsLoading: 0,
        assets: assets,
        interval: null,
        finishQueue: [],
        loadIndex: 0,
        finishIndex: 0,
        pendingScriptLoads: 0,
        jsilInitialized: false,
        assetsLoadingNames: {},
        assetLoadFailures: [],
        failedFinishes: 0
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

    pollAssetQueue(state);
};

function beginLoading() {
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
    if (typeof (window.assetsToLoad) !== "undefined") {
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

    postMessage("Loading data ... ");
    loadAssets(allAssetsToLoad);
};

function registerErrorHandler() {
    var oldErrorHandler = window.onerror;

    window.onerror = function JSIL_OnUnhandledException(errorMsg, url, lineNumber) {
        JSIL.Host.logWriteLine("Unhandled exception at " + url + " line " + lineNumber + ":");
        JSIL.Host.logWriteLine(errorMsg);

        if (typeof (oldErrorHandler) === "function")
            return oldErrorHandler(errorMsg, url, lineNumber);
        else
            return false;
    };
};

function stringifyLoadError(error) {
    if (error && error.statusText)
        return error.statusText;
    else if (
      error &&
      (typeof (error) === "object") &&
      (error.toString().indexOf("[object") === 0)
    )
        return "Unknown error";
    else
        return String(error);
};

function showSaveRecordingDialog() {
    try {
        Microsoft.Xna.Framework.Game.ForcePause();
    } catch (exc) {
    }

    var theDialog = document.getElementById("saveRecordingDialog");
    if (!theDialog) {
        var dialog = document.createElement("div");
        dialog.id = "saveRecordingDialog";

        dialog.innerHTML =
          '<label for="recordingName">Recording Name:</label> ' +
          '<input type="text" id="recordingName" value="test" style="background: white; color: black"><br>' +
          '<a id="saveRecordingToLocalStorage" href="#" style="color: black">Save to Local Storage</a> | ' +
          '<a id="saveRecordingAsFile" download="test.replay" target="_blank" href="#" style="color: black">Download</a> | ' +
          '<a id="cancelSaveRecording" href="#" style="color: black">Close</a>';

        dialog.style.position = "absolute";
        dialog.style.background = "rgba(240, 240, 240, 0.9)";
        dialog.style.color = "black";
        dialog.style.padding = "24px";
        dialog.style.borderRadius = "8px 8px 8px 8px";
        dialog.style.boxShadow = "2px 2px 4px rgba(0, 0, 0, 0.75)";

        var body = document.getElementsByTagName("body")[0];

        body.appendChild(dialog);
        theDialog = dialog;

        document.getElementById("saveRecordingToLocalStorage").addEventListener("click", saveRecordingToLocalStorage, true);
        document.getElementById("cancelSaveRecording").addEventListener("click", hideSaveRecordingDialog, true);

        var inputField = document.getElementById("recordingName")
        inputField.addEventListener("input", updateSaveLinkDownloadAttribute, true);
        inputField.addEventListener("change", updateSaveLinkDownloadAttribute, true);
        inputField.addEventListener("blur", updateSaveLinkDownloadAttribute, true);
    }

    var saveLink = document.getElementById("saveRecordingAsFile");

    try {
        // FIXME: Memory leak
        var json = JSIL.Replay.SaveAsJSON();
        var bytes = JSIL.StringToByteArray(json);

        saveLink.href = JSIL.GetObjectURLForBytes(bytes, "application/json");
    } catch (exc) {
    }

    var x = (document.documentElement.clientWidth - theDialog.clientWidth) / 2;
    var y = (document.documentElement.clientHeight - theDialog.clientHeight) / 2;
    theDialog.style.left = x + "px";
    theDialog.style.top = y + "px";
    theDialog.style.display = "block";
};

function updateSaveLinkDownloadAttribute(evt) {
    var saveLink = document.getElementById("saveRecordingAsFile");
    var recordingName = document.getElementById("recordingName").value.trim() || "untitled";

    saveLink.download = recordingName + ".replay";
};

function saveRecordingToLocalStorage(evt) {
    if (evt) {
        evt.preventDefault();
        evt.stopPropagation();
    }

    JSIL.Replay.SaveToLocalStorage(document.getElementById("recordingName").value.trim() || "untitled");
};

function hideSaveRecordingDialog(evt) {
    if (evt) {
        evt.preventDefault();
        evt.stopPropagation();
    }

    var theDialog = document.getElementById("saveRecordingDialog");
    theDialog.style.display = "none";

    try {
        Microsoft.Xna.Framework.Game.ForceUnpause();
    } catch (exc) {
    }
};

JSIL.WebWorker.OneShotEventListenerCount = 0;

JSIL.WebWorker.$MakeWrappedListener = function (listener, notification) {
    return function WrappedEventListener() {
        notification();

        return listener.apply(this, arguments);
    };
};

JSIL.WebWorker.RegisterOneShotEventListener = function (element, eventName, capture, listener) {
    var registered = true;
    var unregister, wrappedListener;

    unregister = function () {
        if (registered) {
            registered = false;
            element.removeEventListener(eventName, wrappedListener, capture);
            JSIL.WebWorker.OneShotEventListenerCount -= 1;

            wrappedListener = null;
            element = null;
        }
    };

    wrappedListener = JSIL.WebWorker.$MakeWrappedListener(listener, unregister);
    listener = null;

    JSIL.WebWorker.OneShotEventListenerCount += 1;
    element.addEventListener(eventName, wrappedListener, capture);

    return {
        eventName: eventName,
        unregister: unregister
    }
};

