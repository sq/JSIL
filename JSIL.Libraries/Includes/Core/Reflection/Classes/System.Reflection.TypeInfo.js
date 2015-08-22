JSIL.ImplementExternals(
  "System.Reflection.TypeInfo", function ($) {
      $.RawMethod(false, "$internalCtor",
        function (rawType) {
            this._type = rawType;
        }
      );

      $.Method({ Static: false, Public: true }, "AsType",
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
        function AsType() {
            return this._type;
        }
      );

      $.Method({ Static: false, Public: true, Virtual: true }, "IsAssignableFrom",
        new JSIL.MethodSignature($.Boolean, [$.Type]),
        function IsAssignableFrom(testType) {
            return this._type.IsAssignableFrom(testType);
        }
      )

      $.Method({ Static: false, Public: true }, "get_Assembly",
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.Assembly"), [], []),
        function get_Assembly() {
            return this._type.get_Assembly();
        }
      );
  }
);