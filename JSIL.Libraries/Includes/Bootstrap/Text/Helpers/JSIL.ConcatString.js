JSIL.ConcatString = function (/* ...values */) {
  var result = "";

  if (arguments[0] !== null)
    result = String(arguments[0]);

  for (var i = 1, l = arguments.length; i < l; i++) {
    var arg = arguments[i];
    if (arg === null)
      ;
    else if (typeof (arg) === "string")
      result += arg;
    else if ($jsilcore.JSIL.Box.IsBoxedOfType(arg, $jsilcore.System.Char))
      result += String.fromCharCode(arg.valueOf());
    else
      result += String(arg);
  }

  return result;
};