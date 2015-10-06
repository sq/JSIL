//? include("../Utils/$jsilcore.$MakeParseExternals.js");
//? include("../Utils/$jsilcore.$ParseInt.js");
JSIL.ImplementExternals(
  "System.Int32", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $jsilcore.$MakeParseExternals($, $.Int32, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

    $.Constant({ Public: true, Static: true }, "MaxValue", 2147483647);
    $.Constant({ Public: true, Static: true }, "MinValue", -2147483648);
  }
);
JSIL.MakeNumericType(Number, "System.Int32", true, "Int32Array", JSIL.MakeIConvertibleMethods);
