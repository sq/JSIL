"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core required");

var $jsilstorage = JSIL.DeclareAssembly("JSIL.Storage");

//? include("Classes/VirtualVolume.js"); writeln();
//? include("Classes/VirtualDirectory.js"); writeln();
//? include("Classes/VirtualFile.js"); writeln();
//? include("Classes/VirtualJunction.js"); writeln();


JSIL.ImplementExternals("System.IO.FileStream", function ($) {
    $.RawMethod(false, "$fromVirtualFile", function (virtualFile, fileMode, autoFlush) {
        System.IO.Stream.prototype._ctor.call(this);

        this._fileName = virtualFile.path;
        this._buffer = JSIL.Array.Clone(virtualFile.readAllBytes());

        this._pos = 0;
        this._length = this._buffer.length;

        this._canRead = true;
        this._canWrite = true;

        this._onClose = function () {
            if (this._modified && this._buffer) {
                var resultBuffer = JSIL.Array.New(System.Byte, this._length);
                JSIL.Array.CopyTo(this._buffer, resultBuffer, 0);

                virtualFile.writeAllBytes(resultBuffer);

                if (autoFlush)
                    virtualFile.volume.flush();

                this._buffer = null;
            }
        };

        this.$applyMode(fileMode);
    });
});


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