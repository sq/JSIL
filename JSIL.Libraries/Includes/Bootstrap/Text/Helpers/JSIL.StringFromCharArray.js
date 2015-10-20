JSIL.StringFromCharArray = function (chars, startIndex, length) {
  var result = "";

  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = chars.length;

  for (var i = startIndex; i < length - startIndex; i++) {
    result += String.fromCharCode(chars[i]);
  }

  return result;
};