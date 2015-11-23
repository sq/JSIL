JSIL.MakeClass("System.Object", "System.Runtime.CompilerServices.CallSite", true, [], function ($) {
});

JSIL.MakeClass("System.Object", "System.Runtime.CompilerServices.CallSite`1", true, ["T"], function ($) {
  $.Method({ Static: true, Public: true }, "Create",
    new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Runtime.CompilerServices.CallSiteBinder")], []),
    function(binder) {
      var callSite = new this();
      callSite.Target = binder.Method;
      return callSite;
    }
  );

  $.Field({ Public: false, Static: false }, "Target", new JSIL.GenericParameter("T", "System.Runtime.CompilerServices.CallSite`1"));
});