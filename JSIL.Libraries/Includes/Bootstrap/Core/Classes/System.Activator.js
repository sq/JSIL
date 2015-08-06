JSIL.ImplementExternals("System.Activator", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({ Static: true, Public: true }, "CreateInstance",
    (new JSIL.MethodSignature($.Object, [mscorlib.TypeRef("System.Type")], [])),
    function CreateInstance(type) {
      return JSIL.CreateInstanceOfType(type, "_ctor", []);
    }
  );

  $.Method({ Static: true, Public: true }, "CreateInstance",
    (new JSIL.MethodSignature($.Object, [mscorlib.TypeRef("System.Type"), mscorlib.TypeRef("System.Array", [$.Object])], [])),
    function CreateInstance(type, args) {
      if (!args)
        args = [];

      return JSIL.CreateInstanceOfType(type, "_ctor", args);
    }
  );

  $.Method({ Static: true, Public: true }, "CreateInstance",
    (new JSIL.MethodSignature("!!0", [], ["T"])),
    function CreateInstance(T) {
      return JSIL.CreateInstanceOfType(T, "_ctor", []);
    }
  );

  $.Method({ Static: true, Public: true }, "CreateInstance",
    (new JSIL.MethodSignature("!!0", [mscorlib.TypeRef("System.Array", [$.Object])], ["T"])),
    function CreateInstance(T, args) {
      if (!args)
        args = [];

      return JSIL.CreateInstanceOfType(T, "_ctor", args);
    }
  );

  $.Method({ Static: true, Public: true }, "CreateInstance",
    (new JSIL.MethodSignature($.Object, [
          $jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.Reflection.BindingFlags"),
          $jsilcore.TypeRef("System.Reflection.Binder"), $jsilcore.TypeRef("System.Array", [$.Object]),
          $jsilcore.TypeRef("System.Globalization.CultureInfo")
    ], [])),
    function CreateInstance(type, bindingAttr, binder, args, culture) {
      // FIXME
      if (!args)
        args = [];

      return JSIL.CreateInstanceOfType(type, "_ctor", args);
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Activator", true, [], function ($) {
});
//? }