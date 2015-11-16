//? include("../Utils/$jsilcore.$MakeParseExternals.js");
//? include("../Utils/$jsilcore.$ParseInt.js");
JSIL.ImplementExternals(
  "System.UInt16", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return ((typeof (value) === "number") && (value >= 0)) || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $jsilcore.$MakeParseExternals($, $.UInt16, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

    $.Constant({ Public: true, Static: true }, "MaxValue", 65535);
    $.Constant({ Public: true, Static: true }, "MinValue", 0);
  }
);
JSIL.MakeNumericType(Number, "System.UInt16", true, "Uint16Array", JSIL.MakeIConvertibleMethods);
