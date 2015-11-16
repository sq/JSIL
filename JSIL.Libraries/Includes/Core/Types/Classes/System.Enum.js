JSIL.MakeClass("System.ValueType", "System.Enum", true, [], function ($) {
    $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("System.IConvertible")
    );
});

JSIL.ImplementExternals("System.Enum", function ($) {
    $.Method({ Static: true, Public: true }, "ToObject",
      (new JSIL.MethodSignature($.Object, ["System.Type", $.Int32], [])),
      function ToObject(enumType, value) {
        return enumType.__PublicInterface__[enumType.__ValueToName__[value]];
      }
    );

    $.Method({ Static: true, Public: true }, "ToObject",
      (new JSIL.MethodSignature($.Object, ["System.Type", $.Object], [])),
      function ToObject(enumType, value) {
        return enumType.__PublicInterface__[enumType.__ValueToName__[value.valueOf()]];
      }
    );

    $.Method({ Static: false, Public: false, Virtual: true }, "ToInt32",
      new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IFormatProvider")], []),
      function (provider) {
        var value = this.value;
        if (typeof (value) == "number") {
          value = this.__ThisType__.__StorageType__.__PublicInterface__.$Box(value);
        }

        return $jsilcore.System.Convert.ToInt32(value, provider);
      }
    );

    $.Method({ Static: false, Public: false, Virtual: true }, "ToInt64",
      new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.IFormatProvider")], []),
      function (provider) {
        var value = this.value;
        if (typeof (value) == "number") {
          value = this.__ThisType__.__StorageType__.__PublicInterface__.$Box(value);
        }

        return $jsilcore.System.Convert.ToInt64(value, provider);
      }
    );

    $.Method({ Static: false, Public: true }, "Object.Equals",
      new JSIL.MethodSignature(System.Boolean, [System.Object]),
      function (rhs) {
          if (rhs === null)
              return false;

          return (this.__ThisType__ === rhs.__ThisType__) &&
            (this.value === rhs.value);
      }
    );
});