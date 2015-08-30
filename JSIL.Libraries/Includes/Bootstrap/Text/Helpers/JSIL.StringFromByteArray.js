JSIL.StringFromByteArray = function (bytes, startIndex, length) {
  var result = "";

  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = bytes.length;

  for (var i = 0; i < length; i++) {
    var ch = bytes[i + startIndex] | 0;

    result += String.fromCharCode(ch);
  }

  return result;
};