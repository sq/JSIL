//? include("../Utils/$jsilcore.$MakeParseExternals.js");
//? include("../Utils/$jsilcore.$ParseInt.js");
JSIL.ImplementExternals(
  "System.Int16", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $jsilcore.$MakeParseExternals($, $.Int16, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

    $.Constant({ Public: true, Static: true }, "MaxValue", 32767);
    $.Constant({ Public: true, Static: true }, "MinValue", -32768);
  }
);
JSIL.MakeNumericType(Number, "System.Int16", true, "Int16Array", JSIL.MakeIConvertibleMethods);
