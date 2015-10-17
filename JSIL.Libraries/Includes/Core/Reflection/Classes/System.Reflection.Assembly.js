JSIL.MakeClass("System.Object", "System.Reflection.Assembly", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (publicInterface, fullName) {
    JSIL.SetValueProperty(this, "__PublicInterface__", publicInterface);
    JSIL.SetValueProperty(this, "__FullName__", fullName);
  });

  $.Method({ Static: true, Public: true }, "op_Equality",
    (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])),
    function op_Equality(left, right) {
      return left === right;
    }
  );

  $.Method({ Static: true, Public: true }, "op_Inequality",
    (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])),
    function op_Inequality(left, right) {
      return left !== right;
    }
  );

  $.Method({ Static: false, Public: true }, "get_CodeBase",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_CodeBase() {
      // FIXME
      return "CodeBase";
    }
  );

  $.Method({ Static: false, Public: true }, "get_FullName",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_FullName() {
      return this.__FullName__;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Location",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_Location() {
      // FIXME
      return "Location";
    }
  );

  $.Method({ Static: false, Public: true }, "GetName",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.AssemblyName"), [], [])),
    function GetName() {
      if (!this._assemblyName)
        this._assemblyName = new System.Reflection.AssemblyName(this.__FullName__);

      return this._assemblyName;
    }
  );

  $.Method({ Static: false, Public: true }, "GetType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$.String], [])),
    function GetType(name) {
      return JSIL.GetTypeFromAssembly(this, name, null, false);
    }
  );

  $.Method({ Static: false, Public: true }, "GetType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$.String, $.Boolean], [])),
    function GetType(name, throwOnError) {
      return JSIL.GetTypeFromAssembly(this, name, null, throwOnError);
    }
  );

  $.Method({ Static: true, Public: true }, "Load",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.Assembly"), [$.String], [])),
    function Load(assemblyName) {
      return JSIL.GetAssembly(assemblyName).__Assembly__;
    }
  );

  $.Method({ Static: false, Public: true }, "GetType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [
          $.String, $.Boolean,
          $.Boolean
    ], [])),
    function GetType(name, throwOnError, ignoreCase) {
      if (ignoreCase)
        throw new Error("ignoreCase not implemented");

      return JSIL.GetTypeFromAssembly(this, name, null, throwOnError);
    }
  );

  $.Method({ Static: false, Public: true }, "GetTypes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), [], [])),
    function GetTypes() {
      return JSIL.GetTypesFromAssembly(this.__PublicInterface__);
    }
  );

  $.Method({ Static: false, Public: true }, "get_DefinedTypes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IEnumerable", [$jsilcore.TypeRef("System.TypeInfo")]), [], [])),
    function get_DefinedTypes() {
        return JSIL.GetTypesFromAssembly(this.__PublicInterface__);
    }
  );


  $.Method({ Static: true, Public: true }, "GetEntryAssembly",
    (new JSIL.MethodSignature($.Type, [], [])),
    function GetEntryAssembly() {
      // FIXME: Won't work if multiple loaded assemblies contain entry points.
      for (var k in JSIL.$EntryPoints) {
        var ep = JSIL.$EntryPoints[k];
        return ep[0].__Assembly__;
      }

      return null;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "GetManifestResourceStream",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [$.String], []),
    function GetManifestResourceStream(name) {
      var assemblyKey = this.__FullName__;
      var firstComma = assemblyKey.indexOf(",");
      if (firstComma)
        assemblyKey = assemblyKey.substr(0, firstComma);

      var files = allManifestResources[assemblyKey];
      if (!files)
        throw new Error("Assembly '" + assemblyKey + "' has no manifest resources");

      var fileKey = name.toLowerCase();

      var bytes = files[fileKey];
      if (!bytes)
        throw new Error("No stream named '" + name + "'");

      var result = new System.IO.MemoryStream(bytes, false);
      return result;
    }
  );

  $.Property({ Static: false, Public: true }, "CodeBase");
  $.Property({ Static: false, Public: true }, "Location");
  $.Property({ Static: false, Public: true }, "FullName");
  $.Property({ Static: false, Public: true }, "DefinedTypes");
});