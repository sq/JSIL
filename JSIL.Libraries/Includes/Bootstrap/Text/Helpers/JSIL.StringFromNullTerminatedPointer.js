JSIL.StringFromNullTerminatedPointer = function (chars) {
  var result = "";

  var i = 0;
  while (true) {
    var ch = chars.getElement(i++) | 0;
    if (ch === 0)
      break;

    result += String.fromCharCode(ch);
  }

  return result;
};