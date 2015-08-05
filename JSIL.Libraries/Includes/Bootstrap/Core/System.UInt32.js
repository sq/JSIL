// #include_once "Bootstrap/Core/Utils/$jsilcore.$MakeParseExternals.js"
// #include_once "Bootstrap/Core/Utils/$jsilcore.$ParseInt.js"

JSIL.ImplementExternals(
  "System.UInt32", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") && (value >= 0);
    });

    $jsilcore.$MakeParseExternals($, $.UInt32, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

    $.Constant({ Public: true, Static: true }, "MaxValue", 4294967295);
    $.Constant({ Public: true, Static: true }, "MinValue", 0);
  }
);
JSIL.MakeNumericType(Number, "System.UInt32", true, "Uint32Array");