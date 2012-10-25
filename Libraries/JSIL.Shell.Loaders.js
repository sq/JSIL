var assetLoaders = {
  "Library": function loadLibrary (filename, data) {
    var uri = jsilConfig.libraryRoot + filename;
    load(uri);
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    var uri = jsilConfig.scriptRoot + filename;
    load(uri);
  },
};

function initAssetLoaders () {
};