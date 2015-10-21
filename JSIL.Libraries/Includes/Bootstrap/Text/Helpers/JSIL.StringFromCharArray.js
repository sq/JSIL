JSIL.StringFromCharArray = function (chars, startIndex, length) {
  var result = "";

  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = chars.length;

  for (var i = 0; i < length; i++) {
    result += String.fromCharCode(chars[startIndex + i]);
  }

  return result;
};