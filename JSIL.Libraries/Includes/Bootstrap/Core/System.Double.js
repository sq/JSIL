﻿// #include_once "Bootstrap/Core/Utils/$jsilcore.$MakeParseExternals.js"
// #include_once "Bootstrap/Core/Utils/$jsilcore.$ParseFloat.js"

JSIL.ImplementExternals(
  "System.Double", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number");
    });

    $jsilcore.$MakeParseExternals($, $.Single, $jsilcore.$ParseFloat, $jsilcore.$TryParseFloat);

    $.Constant({ Public: true, Static: true }, "MinValue", -1.7976931348623157E+308);
    $.Constant({ Public: true, Static: true }, "MaxValue", 1.7976931348623157E+308);
    $.Constant({ Public: true, Static: true }, "Epsilon", 4.94065645841247E-324);
    $.Constant({ Public: true, Static: true }, "NegativeInfinity", -Infinity);
    $.Constant({ Public: true, Static: true }, "PositiveInfinity", Infinity);
    $.Constant({ Public: true, Static: true }, "NaN", NaN);
  }
);
JSIL.MakeNumericType(Number, "System.Double", false, "Float64Array");