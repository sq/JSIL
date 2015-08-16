/*? if (!('$jsilcore_$ParseBoolean' in __out)) { __out.$jsilcore_$ParseBoolean = true; */
$jsilcore.$ParseBoolean = function (text) {
  if (arguments.length !== 1)
    throw new Error("NumberStyles not supported");

  var temp = new JSIL.BoxedVariable(null);
  if ($jsilcore.$TryParseBoolean(text, temp))
    return temp.get();

  throw new System.Exception("Invalid boolean");
};

$jsilcore.$TryParseBoolean = function (text, result) {
  if (text === null) {
      result.set(false);
      return true;
  }

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
/*? }*/