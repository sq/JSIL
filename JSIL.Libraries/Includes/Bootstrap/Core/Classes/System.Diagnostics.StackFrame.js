JSIL.ImplementExternals("System.Diagnostics.StackFrame", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      // FIXME
    }
  );

  $.Method({ Static: false, Public: true }, "GetMethod",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Reflection.MethodBase"), [], [])),
    function GetMethod() {
      // FIXME
      return new System.Reflection.MethodBase();
    }
  );
});