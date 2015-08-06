/*? if (!('$jsilcore_$ParseFloat' in __out)) { __out.$jsilcore_$ParseFloat = true; */
$jsilcore.$ParseFloat = function (text, style) {
  var temp = new JSIL.BoxedVariable(null);
  if ($jsilcore.$TryParseFloat(text, style, temp))
    return temp.get();

  throw new System.Exception("Invalid float");
};

$jsilcore.$TryParseFloat = function (text, style, result) {
  if (arguments.length === 2) {
    result = style;
    style = 0;
  }

  var parsed;
  result.set(parsed = parseFloat(text));

  if (isNaN(parsed)) {
    var lowered = text.toLowerCase();

    if (lowered === "nan") {
      result.set(Number.NaN);
      return true;
    } else if (lowered === "-infinity") {
      result.set(Number.NEGATIVE_INFINITY);
      return true;
    } else if ((lowered === "+infinity") || (lowered === "infinity")) {
      result.set(Number.POSITIVE_INFINITY);
      return true;
    } else {
      return false;
    }
  } else {
    return true;
  }
};
/*? }*/