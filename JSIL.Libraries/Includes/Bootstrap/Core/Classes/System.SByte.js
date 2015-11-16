JSIL.ImplementExternals(
  "System.SByte", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return ((typeof (value) === "number") && (value >= -128) && (value <= 127)) || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $.Constant({ Public: true, Static: true }, "MinValue", -128);
    $.Constant({ Public: true, Static: true }, "MaxValue", 127);
  }
);
JSIL.MakeNumericType(Number, "System.SByte", true, "Int8Array", JSIL.MakeIConvertibleMethods);
