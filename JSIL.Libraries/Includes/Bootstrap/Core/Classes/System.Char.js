JSIL.ImplementExternals(
  "System.Char", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $.Constant({ Public: true, Static: true }, "MaxValue", "\uffff");
    $.Constant({ Public: true, Static: true }, "MinValue", "\0");
  }
);
JSIL.MakeNumericType(String, "System.Char", true, null, JSIL.MakeIConvertibleMethods);