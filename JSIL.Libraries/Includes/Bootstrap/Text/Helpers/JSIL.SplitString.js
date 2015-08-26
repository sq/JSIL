JSIL.SplitString = function (str, separators, options) {
  if (options && options.value)
    JSIL.RuntimeError("StringSplitOptions other than None are not implemented");

  if (!separators) {
    // Whitespace characters from Unicode 6.0
    separators = [
      "\u0009", "\u000A", "\u000B", "\u000C", "\u000D", "\u0020", "\u0085", "\u00A0",
      "\u1680", "\u180E", "\u2000", "\u2001", "\u2002", "\u2003", "\u2004", "\u2005",
      "\u2006", "\u2007", "\u2008", "\u2009", "\u200A", "\u2028", "\u2029", "\u202F",
      "\u205F", "\u3000"
    ];
  }

  if (separators.length === 1) {
    return str.split(separators[0]);
  } else {
    var regexText = "";
    for (var i = 0; i < separators.length; i++) {
      if (i > 0)
        regexText += "|"

      regexText += JSIL.EscapeJSRegex(separators[i]);
    }
    var regex = new RegExp(regexText, "g");

    return str.split(regex);
  }
};