JSIL.StringFromCharArray = function (chars, startIndex, length) {
  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = chars.length;

  if (arguments.length > 1) {
    var arr = chars.slice(startIndex, startIndex + length);
    return arr.join("");
  } else {
    return chars.join("");
  }
};