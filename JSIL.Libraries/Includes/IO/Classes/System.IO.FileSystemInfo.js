JSIL.ImplementExternals("System.IO.FileSystemInfo", function ($) {
    $.RawMethod(false, "$fromNodeAndPath", function (node, path) {
        this._node = node;

        if (node)
            this._path = node.path;
        else
            this._path = path;
    });

    $.Method({ Static: false, Public: false }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
          this._node = null;
          this._path = null;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Exists",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_Exists() {
          return (this._node !== null);
      }
    );

    $.Method({ Static: false, Public: true }, "get_Extension",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Extension() {
          return System.IO.Path.GetExtension(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "get_FullName",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_FullName() {
          return this._path;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Name",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Name() {
          return System.IO.Path.GetFileName(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "Refresh",
      (JSIL.MethodSignature.Void),
      function Refresh() {
          // FIXME: Does this need to do anything?
      }
    );
});