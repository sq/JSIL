/*? if (!('$jsilcore_$ParseInt' in __out)) { __out.$jsilcore_$ParseInt = true; */
$jsilcore.$ParseInt = function (text, style) {
  var temp = new JSIL.BoxedVariable(null);
  if ($jsilcore.$TryParseInt(text, style, temp))
    return temp.get();

  throw new System.FormatException("Invalid integer");
};

$jsilcore.$TryParseInt = function (text, style, result) {
  if (arguments.length === 2) {
    result = style;
    style = 0;
  }

  var radix = 10;

  if (style & System.Globalization.NumberStyles.AllowHexSpecifier)
    radix = 16;

  var parsed;
  result.set(parsed = parseInt(text, radix));
  return !isNaN(parsed);
};
/*? }*/