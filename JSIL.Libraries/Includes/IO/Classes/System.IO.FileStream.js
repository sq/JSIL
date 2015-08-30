JSIL.ImplementExternals("System.IO.FileStream", function ($) {
    $.Method({ Static: false, Public: false }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
          System.IO.Stream.prototype._ctor.call(this);

          this._pos = 0;
          this._length = 0;
          this._canRead = false;
          this._canWrite = false;
      }
    );

    var ctorImpl = function _ctor(path, mode) {
        System.IO.Stream.prototype._ctor.call(this);

        var storageRoot = JSIL.Host.getStorageRoot();

        if (storageRoot) {
            var createNew = (mode == System.IO.FileMode.Create) ||
              (mode == System.IO.FileMode.CreateNew) ||
              (mode == System.IO.FileMode.OpenOrCreate);

            var resolved = storageRoot.resolvePath(path, false);

            if (createNew && !resolved)
                resolved = storageRoot.createFile(path, true);

            if (resolved && resolved.type === "file") {
                this.$fromVirtualFile(resolved, mode, true);
                return;
            }

            throw new System.IO.FileNotFoundException(path);
        }

        throw new System.NotImplementedException("No storage root available");
    };

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.IO.FileMode")], [])),
      ctorImpl
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess")], [])),
      // FIXME: access
      ctorImpl
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess"), $jsilcore.TypeRef("System.IO.FileShare")], [])),
      // FIXME: access, share
      ctorImpl
    );

    $.RawMethod(false, "$applyMode", function (fileMode) {
        var fm = System.IO.FileMode;

        if (
          (fileMode == fm.Create) ||
          (fileMode == fm.Truncate) ||
          (fileMode == fm.CreateNew)
        ) {
            this._buffer = [];
            this._length = 0;
        } else if (fileMode == fm.Append) {
            this._pos = this._length;
        }
    });

    $.RawMethod(false, "$GetURI", function () {
        var slashRe = /\\/g;
        var uri = ("./" + this._fileName).replace(slashRe, "/");

        return uri;
    });

    $.Method({ Static: false, Public: true }, "get_Name",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Name() {
          return this._fileName;
      }
    );
});

JSIL.ImplementExternals(
  "System.IO.FileStream", $bytestream
);
