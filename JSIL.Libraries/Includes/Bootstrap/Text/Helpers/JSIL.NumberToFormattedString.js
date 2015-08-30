JSIL.ParseCustomNumberFormat = function (customFormat) {
  var inQuotedString = false, quoteCharacter = null, stringStartOffset = -1;
  var containsDecimal = false;

  var commands = [];

  var digit = function (state) {
    var digits = state.digits;
    var result = digits.shift();

    if (state.leadingMinus) {
      state.leadingMinus = false;

      if (result !== null)
        result = "-" + result;
      else
        result = "-";
    }

    return result;
  };

  var zeroOrDigit = function (state) {
    var digits = state.digits;
    var digit = digits.shift();

    var result;
    if (digit === null)
      result = "0";
    else
      result = digit;

    if (state.leadingMinus) {
      state.leadingMinus = false;
      result = "-" + result;
    }

    return result;
  };

  var decimal = function (state) {
    state.afterDecimal = true;

    if (state.omitDecimal)
      return null;
    else
      return ".";
  };

  var rawCharacter = function (state) {
    var character = this;

    return character;
  };

  var quotedString = function (state) {
    var text = this;

    return text;
  };

  var includePlaceSeparators = false;
  var digitCount = 0, digitsBeforeDecimal = 0, digitsAfterDecimal = 0, zeroesAfterDecimal = 0;

  for (var i = 0, l = customFormat.length; i < l; i++) {
    var ch = customFormat[i];

    if (inQuotedString) {
      if (ch === quoteCharacter) {
        inQuotedString = false;

        var quotedText = customFormat.substr(stringStartOffset, i - stringStartOffset);
        commands.push(quotedString.bind(quotedText));
      }

      continue;
    }

    switch (ch) {
      case "\t":
      case " ":
        commands.push(rawCharacter.bind(ch));
        break;

      case ",":
        includePlaceSeparators = true;
        break;

      case "'":
      case '"':
        quoteCharacter = ch;
        inQuotedString = true;
        stringStartOffset = i + 1;
        break;

      case '#':
        digitCount++;

        commands.push(digit);
        continue;

      case '0':
        digitCount++;
        if (containsDecimal)
          zeroesAfterDecimal++;

        commands.push(zeroOrDigit);
        continue;

      case '.':
        if (containsDecimal)
          JSIL.RuntimeError("Multiple decimal places in format string");
        else
          containsDecimal = true;

        digitsBeforeDecimal = digitCount;
        digitCount = 0;
        commands.push(decimal);

        continue;

      default:
        return null;
    }
  }

  if (containsDecimal)
    digitsAfterDecimal = digitCount;
  else
    digitsBeforeDecimal = digitCount;

  var formatter = function (value) {
    var formatted = value.toString(10);
    var pieces = formatted.split(".");

    var preDecimal = Array.prototype.slice.call(pieces[0]), postDecimal;
    var actualDigitsAfterDecimal = 0;

    var leadingMinus = preDecimal[0] === "-";
    if (leadingMinus)
      preDecimal.shift();

    if (pieces.length > 1) {
      // If we have too few places after the decimal for all the digits,
      //  we need to recreate the string using toFixed so that it gets rounded.
      if (pieces[1].length > digitsAfterDecimal) {
        pieces = value.toFixed(digitsAfterDecimal).split(".");
      }

      if (digitsAfterDecimal) {
        postDecimal = Array.prototype.slice.call(pieces[1]);
        actualDigitsAfterDecimal = postDecimal.length;
      } else {
        postDecimal = [];
        actualDigitsAfterDecimal = 0;
      }

    } else
      postDecimal = [];

    while (preDecimal.length < digitsBeforeDecimal)
      preDecimal.unshift(null);

    while (postDecimal.length < digitsAfterDecimal)
      postDecimal.push(null);

    // To properly emulate place separators in integer formatting,
    //  we need to insert the commas into the digits array.
    if (includePlaceSeparators) {
      for (var l = preDecimal.length, i = l - 4; i >= 0; i -= 3) {
        var digit = preDecimal[i];

        if (digit !== null)
          preDecimal[i] = digit + ",";
      }
    }

    // If we don't have enough place markers for all our digits,
    //  we turn the extra digits into a single 'digit' entry so
    //  that they are still outputted.

    if (preDecimal.length > digitsBeforeDecimal) {
      var toRemove = preDecimal.length - digitsBeforeDecimal;
      var removed = preDecimal.splice(digitsBeforeDecimal, toRemove).join("");

      var preDecimalDigit = preDecimal[preDecimal.length - 1];

      if (preDecimalDigit !== null)
        preDecimal[preDecimal.length - 1] = preDecimalDigit + removed;
      else
        preDecimal[preDecimal.length - 1] = removed;
    }

    var state = {
      afterDecimal: false,
      omitDecimal: (actualDigitsAfterDecimal <= 0) && (zeroesAfterDecimal <= 0),
      leadingMinus: leadingMinus
    };

    Object.defineProperty(
      state, "digits", {
        configurable: false,
        enumerable: true,

        get: function () {
          if (state.afterDecimal)
            return postDecimal;
          else
            return preDecimal;
        }
      }
    );

    var result = "";

    for (var i = 0, l = commands.length; i < l; i++) {
      var command = commands[i];

      var item = command(state);
      if (item)
        result += item;
    }

    return result;
  };

  return formatter;
};

JSIL.NumberToFormattedString = function (value, alignment, valueFormat, formatProvider) {
  // FIXME: formatProvider

  if (
    (arguments.length === 1) ||
    ((typeof (alignment) === "undefined") && (typeof (valueFormat) === "undefined"))
  )
    return value.toString();

  var formatInteger = function (value, radix, digits) {
    digits = parseInt(digits);
    if (isNaN(digits))
      digits = 0;

    var result = parseInt(value).toString(radix);

    while (result.length < digits)
      result = "0" + result;

    return result;
  };

  var formatFloat = function (value, digits) {
    digits = parseInt(digits);
    if (isNaN(digits))
      digits = 2;

    return parseFloat(value).toFixed(digits);
  };

  var insertPlaceSeparators = function (valueString) {
    var pieces = valueString.split(".");

    var newIntegralPart = "";

    for (var i = 0, l = pieces[0].length; i < l; i++) {
      var ch = pieces[0][i];
      var p = (l - i) % 3;

      if ((i > 0) && (p === 0))
        newIntegralPart += ",";

      newIntegralPart += ch;
    }

    pieces[0] = newIntegralPart;

    return pieces.join(".");
  };

  var parsedCustomFormat = null;
  var result;

  if (valueFormat)
    parsedCustomFormat = JSIL.ParseCustomNumberFormat(valueFormat);

  if (parsedCustomFormat) {
    result = parsedCustomFormat(value);

  } else if (valueFormat) {
    switch (valueFormat[0]) {
      case 'd':
      case 'D':
        result = formatInteger(value, 10, valueFormat.substr(1));
        break;

      case 'x':
        result = formatInteger(value >>> 0, 16, valueFormat.substr(1)).toLowerCase();
        break;

      case 'X':
        result = formatInteger(value >>> 0, 16, valueFormat.substr(1)).toUpperCase();
        break;

      case 'f':
      case 'F':
        result = formatFloat(value, valueFormat.substr(1));
        break;

      case 'n':
      case 'N':
        result = formatFloat(value, valueFormat.substr(1));
        result = insertPlaceSeparators(result);
        break;

      default:
        JSIL.RuntimeError("Unsupported format string: " + valueFormat);

    }

  } else {
    result = String(value);
  }

  if (typeof (alignment) === "string")
    alignment = parseInt(alignment);

  if (typeof (alignment) === "number") {
    var absAlignment = Math.abs(alignment);
    if (result.length >= absAlignment)
      return result;

    var paddingSize = absAlignment - result.length;
    var padding = "";
    for (var i = 0; i < paddingSize; i++)
      padding += " ";

    if (alignment > 0)
      return padding + result;
    else
      return result + padding;
  }

  return result;
};