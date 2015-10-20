JSIL.SplitString = function (str, separators, options) {
  if (options && options.value)
    JSIL.RuntimeError("StringSplitOptions other than None are not implemented");

  if (!separators) {
    // Whitespace characters from Unicode 6.0
    separators = [
      0x0009, 0x000A, 0x000B, 0x000C, 0x000D, 0x0020, 0x0085, 0x00A0,
      0x1680, 0x180E, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005,
      0x2006, 0x2007, 0x2008, 0x2009, 0x200A, 0x2028, 0x2029, 0x202F,
      0x205F, 0x3000
    ];
  }

  if (separators.length === 1) {
    return str.split(String.fromCharCode(separators[0]));
  } else {
    var regexText = "";
    for (var i = 0; i < separators.length; i++) {
      if (i > 0)
        regexText += "|"

      regexText += JSIL.EscapeJSRegex(String.fromCharCode(separators[i]));
    }
    var regex = new RegExp(regexText, "g");

    return str.split(regex);
  }
};