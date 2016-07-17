JSIL.SplitString = function (str, separators, count, options) {
  if (options && options.value)
    JSIL.RuntimeError("StringSplitOptions other than None are not implemented");
  if (count > 0 && separators.length > 1)
    JSIL.RuntimeError("Split with count and multiple separators is not implemented");

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
    if (count > 0) {
      var splits = str.split(separators[0]);
      if (splits.length <= count)
        return splits;
      splits.splice(count - 1, splits.length,
          splits.slice(count - 1).join(separators[0]));
      return splits;
    } else {
      return str.split(separators[0]);
    }
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