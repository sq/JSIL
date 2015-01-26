var assetLoaders = {
  "Library": function loadLibrary (filename, data) {
    var uri = JSIL.Shell.GetURIForFile(jsilConfig.libraryRoot, filename);
    load(uri);
  },
  "Script": function loadScript (filename, data, onError, onDoneLoading, state) {
    var uri = JSIL.Shell.GetURIForFile(jsilConfig.scriptRoot, filename);
    load(uri);
  },
};

JSIL.Shell.GetURIForFile = function (root, filename) {
    if (
        (filename.indexOf(":") > 0) || 
        (filename[0] == "/")
    )
        return filename;
    else
        return root + filename;
};

function initAssetLoaders () {
};