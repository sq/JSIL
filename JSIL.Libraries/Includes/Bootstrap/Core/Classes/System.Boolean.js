//? include("../Utils/$jsilcore.$MakeParseExternals.js");
//? include("../Utils/$jsilcore.$ParseBoolean.js");
JSIL.ImplementExternals(
  "System.Boolean", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (value === false) || (value === true) || JSIL.Box.IsBoxedOfType(value, $.Type);
    });

    $jsilcore.$MakeParseExternals($, $.Boolean, $jsilcore.$ParseBoolean, $jsilcore.$TryParseBoolean);
  }
);
JSIL.MakeNumericType(Boolean, "System.Boolean", true, null, JSIL.MakeIConvertibleMethods);