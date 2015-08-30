JSIL.MakeClass("System.Object", "System.ValueType", true, [], function ($) {
    $.Method({ Static: false, Public: true }, "Object.Equals",
      new JSIL.MethodSignature(System.Boolean, [System.Object]),
      function (rhs) {
          return JSIL.StructEquals(this, rhs);
      }
    );
});