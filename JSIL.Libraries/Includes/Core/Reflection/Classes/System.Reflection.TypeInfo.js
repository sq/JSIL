JSIL.MakeClass("System.Type", "System.Reflection.TypeInfo", false, [], function ($) {
});

JSIL.ImplementExternals(
  "System.Reflection.TypeInfo", function ($) {
      $.Method({ Static: false, Public: true }, "AsType",
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
        function AsType() {
            return this;
        }
      );
  }
);