// #include_once "Bootstrap/Core/Utils/$jsilcore.$MakeParseExternals.js"

$jsilcore.$ParseBoolean = function (text) {
    if (arguments.length !== 1)
        throw new Error("NumberStyles not supported");

    var temp = new JSIL.BoxedVariable(null);
    if ($jsilcore.$TryParseBoolean(text, temp))
        return temp.get();

    throw new System.Exception("Invalid boolean");
};

$jsilcore.$TryParseBoolean = function (text, result) {
    text = text.toLowerCase().trim();

    if (text === "true") {
        result.set(true);
        return true;
    } else if (text === "false") {
        result.set(false);
        return true;
    }

    return false;
};

JSIL.ImplementExternals(
  "System.Boolean", function ($) {
      $.RawMethod(true, "CheckType", function (value) {
          return (value === false) || (value === true);
      });

      $jsilcore.$MakeParseExternals($, $.Boolean, $jsilcore.$ParseBoolean, $jsilcore.$TryParseBoolean);
  }
);
JSIL.MakeNumericType(Boolean, "System.Boolean", true);