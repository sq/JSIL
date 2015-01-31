var assetLoaders = {
  "Library": function loadLibrary (filename, data, onError, onDoneLoading, state) {
    var uri = jsilConfig.libraryRoot + filename;
    importScripts(uri);
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    var uri = jsilConfig.scriptRoot + filename;
    importScripts(uri);
  },
};

function initAssetLoaders () {
};