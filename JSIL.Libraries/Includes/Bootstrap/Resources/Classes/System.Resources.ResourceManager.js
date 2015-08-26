JSIL.ImplementExternals("System.Resources.ResourceSet", function ($) {

  $.RawMethod(false, "$fromResources", function (manager, resources) {
    this._manager = manager;
    this._resources = resources;
  });

  $.Method({ Static: false, Public: true }, "Close",
    (JSIL.MethodSignature.Void),
    function Close() {
    }
  );

  $.Method({ Static: false, Public: false }, "Dispose",
    (new JSIL.MethodSignature(null, [$.Boolean], [])),
    function Dispose(disposing) {
    }
  );

  $.Method({ Static: false, Public: true }, "Dispose",
    (JSIL.MethodSignature.Void),
    function Dispose() {
    }
  );

  $.RawMethod(false, "$get", function (key, ignoreCase) {
    if (ignoreCase)
      JSIL.RuntimeError("Case insensitive resource fetches not implemented");

    var result = this._resources[key];
    if (!result)
      return null;

    return result;
  });

  $.Method({ Static: false, Public: true }, "GetObject",
    (new JSIL.MethodSignature($.Object, [$.String], [])),
    function GetObject(name) {
      return this.$get(name, false);
    }
  );

  $.Method({ Static: false, Public: true }, "GetObject",
    (new JSIL.MethodSignature($.Object, [$.String, $.Boolean], [])),
    function GetObject(name, ignoreCase) {
      return this.$get(name, ignoreCase);
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [$.String], [])),
    function GetString(name) {
      var result = this.$get(name, false);
      if (typeof (result) !== "string")
        return null;

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [$.String, $.Boolean], [])),
    function GetString(name, ignoreCase) {
      var result = this.$get(name, ignoreCase);
      if (typeof (result) !== "string")
        return null;

      return result;
    }
  );

});