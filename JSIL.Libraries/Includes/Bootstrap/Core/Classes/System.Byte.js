JSIL.ImplementExternals(
  "System.Byte", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return ((typeof (value) === "number") && (value >= 0) && (value <= 255))
        || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $.Constant({ Public: true, Static: true }, "MinValue", 0);
    $.Constant({ Public: true, Static: true }, "MaxValue", 255);
  }
);
JSIL.MakeNumericType(Number, "System.Byte", true, "Uint8Array", JSIL.MakeIConvertibleMethods);
