JSIL.MakeClass("System.Reflection.TypeInfo", "System.RuntimeType", false, [], function ($) {
  $jsilcore.RuntimeTypeInitialized = true;

  $.Method({ Public: true, Static: true }, "op_Equality",
    new JSIL.MethodSignature($.Boolean, [$.Type, $.Type]),
    function(lhs, rhs) {
      if (lhs === rhs)
        return true;

      return String(lhs) == String(rhs);
    }
  );
});