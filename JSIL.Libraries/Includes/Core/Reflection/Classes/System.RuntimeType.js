JSIL.MakeClass("System.Reflection.TypeInfo", "System.RuntimeType", false, [], function ($) {
  $jsilcore.RuntimeTypeInitialized = true;

  $.Method({ Public: true, Static: true }, "op_Equality",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.RuntimeType"), $jsilcore.TypeRef("System.RuntimeType")]),
    function(lhs, rhs) {
      if (lhs === rhs)
        return true;

      return String(lhs) == String(rhs);
    }
  );
});