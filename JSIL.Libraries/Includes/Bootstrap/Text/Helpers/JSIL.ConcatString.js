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
    else
      result += String(arg);
  }

  return result;
};