"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core required");

var $jsilstorage = JSIL.DeclareAssembly("JSIL.Storage");

//? include("Classes/VirtualVolume.js"); writeln();
//? include("Classes/VirtualDirectory.js"); writeln();
//? include("Classes/VirtualFile.js"); writeln();
//? include("Classes/VirtualJunction.js"); writeln();

//? include("Classes/System.IO.FileStream.js"); writeln();

$jsilstorage.providers = [];

JSIL.RegisterStorageProvider = function (provider) {
    $jsilstorage.providers.push(provider);
};

JSIL.GetStorageVolumes = function () {
    var result = [];

    for (var i = 0, l = $jsilstorage.providers.length; i < l; i++) {
        var provider = $jsilstorage.providers[i];

        var volumes = provider.getVolumes();

        for (var j = 0, m = volumes.length; j < m; j++)
            result.push(volumes[j]);
    }

    return result;
};