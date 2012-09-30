"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("System.ComponentModel");
JSIL.DeclareNamespace("System.Linq");
JSIL.DeclareNamespace("System.IO");
JSIL.DeclareNamespace("System.Text.RegularExpressions");

// Unfortunately necessary :-(
String.prototype.Object_Equals = function (rhs) {
  return this === rhs;
};

$jsilcore.$ParseBoolean = function (text) {
  if (arguments.length !== 1)
    throw new Error("NumberStyles not supported");

  var temp = {};
  if ($jsilcore.$TryParseBoolean(text, temp))
    return temp.value;

  throw new System.Exception("Invalid boolean");
};

$jsilcore.$TryParseBoolean = function (text, result) {
  text = text.toLowerCase().trim();

  if (text === "true") {
    result.value = true;
    return true;
  } else if (text === "false") {
    result.value = false;
    return true;
  }

  return false;
};

$jsilcore.$MakeParseExternals = function ($, type, parse, tryParse) {
  $.Method({Static:true , Public:true }, "Parse", 
    (new JSIL.MethodSignature(type, [$.String], [])), 
    parse
  );

  $.Method({Static:true , Public:true }, "Parse", 
    (new JSIL.MethodSignature(type, [$.String, $jsilcore.TypeRef("System.Globalization.NumberStyles")], [])), 
    parse
  );

  $.Method({Static:true , Public:true }, "TryParse", 
    (new JSIL.MethodSignature($.Boolean, [$.String, $jsilcore.TypeRef("JSIL.Reference", [type])], [])), 
    tryParse
  );
};

JSIL.ImplementExternals(
  "System.Boolean", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (value === false) || (value === true);
    });

    $jsilcore.$MakeParseExternals($, $.Boolean, $jsilcore.$ParseBoolean, $jsilcore.$TryParseBoolean);
  }
);
JSIL.MakeNumericType(Boolean, "System.Boolean", true);

JSIL.ImplementExternals(
  "System.Char", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "string") && (value.length == 1);
    });

		$.Constant({Public: true, Static: true}, "MaxValue", "\uffff");
		$.Constant({Public: true, Static: true}, "MinValue", "\0");
  }
);
JSIL.MakeNumericType(String, "System.Char", true);

JSIL.ImplementExternals(
  "System.Byte", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") && (value >= 0) && (value <= 255);
    });
    
		$.Constant({Public: true, Static: true}, "MinValue", 0);
    $.Constant({Public: true, Static: true}, "MaxValue", 255);
  }
);
JSIL.MakeNumericType(Number, "System.Byte", true, "Uint8Array");

JSIL.ImplementExternals(
  "System.SByte", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") && (value >= -128) && (value <= 127);
    });
    
		$.Constant({Public: true, Static: true}, "MinValue", -128);
    $.Constant({Public: true, Static: true}, "MaxValue", 127);
  }
);
JSIL.MakeNumericType(Number, "System.SByte", true, "Int8Array");

$jsilcore.$ParseInt = function (text, style) {
  var temp = {};
  if ($jsilcore.$TryParseInt(text, style, temp))
    return temp.value;

  throw new System.Exception("Invalid integer");
};

$jsilcore.$TryParseInt = function (text, style, result) {
  if (arguments.length === 2) {
    result = style;
    style = 0;
  }

  var radix = 10;

  if (style & System.Globalization.NumberStyles.AllowHexSpecifier)
    radix = 16;

  result.value = parseInt(text, radix);
  return !isNaN(result.value);
};

JSIL.ImplementExternals(
  "System.UInt16", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") && (value >= 0);
    });

    $jsilcore.$MakeParseExternals($, $.UInt16, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

		$.Constant({Public: true, Static: true}, "MaxValue", 65535);
		$.Constant({Public: true, Static: true}, "MinValue", 0);
  }
);
JSIL.MakeNumericType(Number, "System.UInt16", true, "Uint16Array");

JSIL.ImplementExternals(
  "System.Int16", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number");
    });

    $jsilcore.$MakeParseExternals($, $.Int16, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);
    
		$.Constant({Public: true, Static: true}, "MaxValue", 32767);
		$.Constant({Public: true, Static: true}, "MinValue", -32768);
  }
);
JSIL.MakeNumericType(Number, "System.Int16", true, "Int16Array");

JSIL.ImplementExternals(
  "System.UInt32", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number") && (value >= 0);
    });

    $jsilcore.$MakeParseExternals($, $.UInt32, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

		$.Constant({Public: true, Static: true}, "MaxValue", 4294967295);
		$.Constant({Public: true, Static: true}, "MinValue", 0);
  }
);
JSIL.MakeNumericType(Number, "System.UInt32", true, "Uint32Array");

JSIL.ImplementExternals(
  "System.Int32", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number");
    });

    $jsilcore.$MakeParseExternals($, $.Int32, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);
    
		$.Constant({Public: true, Static: true}, "MaxValue", 2147483647);
		$.Constant({Public: true, Static: true}, "MinValue", -2147483648);
  }
);
JSIL.MakeNumericType(Number, "System.Int32", true, "Int32Array");

//JSIL.ImplementExternals(
//  "System.Int64", function ($) {
//    $.RawMethod(true, "CheckType", function (value) {
//      return value.constructor === goog.math.Long;
//    });

//    $jsilcore.$MakeParseExternals($, $.Int64, $jsilcore.$ParseInt, $jsilcore.$TryParseInt);

//		$.Constant({Public: true, Static: true}, "MaxValue", 9223372036854775807);
//		$.Constant({Public: true, Static: true}, "MinValue", -9223372036854775808);
//  }
//);
//JSIL.MakeNumericType(Number, "System.Int64", true, "Int64Array");

$jsilcore.$ParseFloat = function (text, style) {
  var temp = {};
  if ($jsilcore.$TryParseFloat(text, style, temp))
    return temp.value;

  throw new System.Exception("Invalid float");
};

$jsilcore.$TryParseFloat = function (text, style, result) {
  if (arguments.length === 2) {
    result = style;
    style = 0;
  }

  result.value = parseFloat(text);

  if (isNaN(result.value)) {
    var lowered = text.toLowerCase();

    if (lowered === "nan") {
      result.value = Number.NaN;
      return true;
    } else if (lowered === "-infinity") {
      result.value = Number.NEGATIVE_INFINITY;
      return true;
    } else if ((lowered === "+infinity") || (lowered === "infinity")) {
      result.value = Number.POSITIVE_INFINITY;
      return true;
    } else {
      return false;
    }
  } else {
    return true;
  }
};

JSIL.ImplementExternals(
  "System.Single", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number");
    });

    $jsilcore.$MakeParseExternals($, $.Single, $jsilcore.$ParseFloat, $jsilcore.$TryParseFloat);

		$.Constant({Public: true, Static: true}, "MinValue", -3.4028234663852886E+38);
		$.Constant({Public: true, Static: true}, "Epsilon", 1.4012984643248171E-45);
		$.Constant({Public: true, Static: true}, "MaxValue", 3.4028234663852886E+38);
		$.Constant({Public: true, Static: true}, "PositiveInfinity", Infinity);
		$.Constant({Public: true, Static: true}, "NegativeInfinity", -Infinity);
		$.Constant({Public: true, Static: true}, "NaN", NaN);
  }
);
JSIL.MakeNumericType(Number, "System.Single", false, "Float32Array");

JSIL.ImplementExternals(
  "System.Double", function ($) {
    $.RawMethod(true, "CheckType", function (value) {
      return (typeof (value) === "number");
    });

    $jsilcore.$MakeParseExternals($, $.Single, $jsilcore.$ParseFloat, $jsilcore.$TryParseFloat);

		$.Constant({Public: true, Static: true}, "MinValue", -1.7976931348623157E+308);
		$.Constant({Public: true, Static: true}, "MaxValue", 1.7976931348623157E+308);
		$.Constant({Public: true, Static: true}, "Epsilon", 4.94065645841247E-324);
		$.Constant({Public: true, Static: true}, "NegativeInfinity", -Infinity);
		$.Constant({Public: true, Static: true}, "PositiveInfinity", Infinity);
		$.Constant({Public: true, Static: true}, "NaN", NaN);
  }
);
JSIL.MakeNumericType(Number, "System.Double", false, "Float64Array");

JSIL.ParseCustomNumberFormat = function (customFormat) {
  var inQuotedString = false, quoteCharacter = null, stringStartOffset = -1;
  var containsDecimal = false;

  var commands = [];

  var digit = function (state) {
    var digits = state.digits;
    return digits.shift();
  };

  var zeroOrDigit = function (state) {
    var digits = state.digits;
    var digit = digits.shift();

    if (digit === null)
      return "0";
    else
      return digit;
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
          throw new Error("Multiple decimal places in format string");
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

    if (pieces.length > 1) {
      // If we have too few places after the decimal for all the digits,
      //  we need to recreate the string using toFixed so that it gets rounded.
      if (pieces[1].length > digitsAfterDecimal)
        pieces = value.toFixed(digitsAfterDecimal).split(".");

      postDecimal = Array.prototype.slice.call(pieces[1]);

      actualDigitsAfterDecimal = postDecimal.length;

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

      preDecimal[preDecimal.length - 1] += removed;
    }

    var state = {
      afterDecimal: false,
      omitDecimal: (actualDigitsAfterDecimal <= 0) && (zeroesAfterDecimal <= 0)
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

JSIL.NumberToFormattedString = function (value, valueFormat, formatProvider) {
  // FIXME: formatProvider

  if (!valueFormat)
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

  var parsedCustomFormat = JSIL.ParseCustomNumberFormat(valueFormat);

  if (parsedCustomFormat) {
    return parsedCustomFormat(value);

  } else {
    switch (valueFormat[0]) {
      case 'd':
      case 'D':
        return formatInteger(value, 10, valueFormat.substr(1));

      case 'x':
        return formatInteger(value, 16, valueFormat.substr(1)).toLowerCase();

      case 'X':
        return formatInteger(value, 16, valueFormat.substr(1)).toUpperCase();

      case 'f':
      case 'F':
        return formatFloat(value, valueFormat.substr(1));

      case 'n':
      case 'N':
        var result = formatFloat(value, valueFormat.substr(1));
        return insertPlaceSeparators(result);

      default:
        throw new Error("Unsupported format string: " + valueFormat);

    }
  }
};

JSIL.StringFromByteArray = function (bytes, startIndex, length) {
  var result = "";

  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = bytes.length;

  for (var i = 0; i < length; i++)
    result += String.fromCharCode(bytes[i + startIndex]);

  return result;
};

JSIL.StringFromCharArray = function (chars, startIndex, length) {
  if (arguments.length < 2)
    startIndex = 0;
  if (arguments.length < 3)
    length = chars.length;

  if (arguments.length > 1) {
    var arr = chars.slice(startIndex, length);
    return arr.join("");
  } else {
    return chars.join("");
  }
};

JSIL.ImplementExternals(
  "System.String", function ($) {
    $.Method({Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [System.Array.Of($jsilcore.TypeRef("System.Char")), "System.Int32", "System.Int32"], [], $jsilcore),
      JSIL.StringFromCharArray
    );

    $.Method({Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [System.Array.Of($jsilcore.TypeRef("System.Char"))], [], $jsilcore),
      function (chars) {
        return JSIL.StringFromCharArray(chars, 0, chars.length);
      }
    );

    $.Method({Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, ["System.Char", "System.Int32"], [], $jsilcore),
      function (ch, length) {
        var arr = new Array(length);
        for (var i = 0; i < length; i++)
          arr[i] = ch;
        
        return arr.join("");
      }
    );

    $.RawMethod(true, "CheckType",
      function (value) {
        return (typeof (value) === "string");
      }
    );

    var compareInternal = function (lhs, rhs, comparison) {
      switch (comparison.valueOf()) {
        case 1: // System.StringComparison.CurrentCultureIgnoreCase:
        case 3: // System.StringComparison.InvariantCultureIgnoreCase:
        case 5: // System.StringComparison.OrdinalIgnoreCase:
          lhs = lhs.toLowerCase();
          rhs = rhs.toLowerCase();
          break;
      }

      if (lhs < rhs)
        return -1;
      else if (lhs > rhs)
        return 1;
      else
        return 0;
    };

    $.Method({Static:true , Public:true }, "Compare", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String")], []),
      function (lhs, rhs) {
        return compareInternal(lhs, rhs, System.StringComparison.Ordinal);
      }
    );

    $.Method({Static:true , Public:true }, "Compare", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String"), 
          $jsilcore.TypeRef("System.Boolean")
        ], []),
      function (lhs, rhs, ignoreCase) {
        return compareInternal(
          lhs, rhs, ignoreCase ? 
            System.StringComparison.OrdinalIgnoreCase : 
            System.StringComparison.Ordinal
        );
      }
    );

    $.Method({Static:true , Public:true }, "Compare", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String"), 
          $jsilcore.TypeRef("System.StringComparison")
        ], []),
      compareInternal
    );

    var concatInternal = function (firstValue) {
      if (JSIL.IsArray(firstValue) && arguments.length == 1) {
        return JSIL.ConcatString.apply(null, firstValue);
      } else {
        return JSIL.ConcatString(Array.prototype.slice.call(arguments));
      }
    };

    $.Method({Static:true , Public:true }, "Concat", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["T"]),
      concatInternal
    );

    $.Method({Static:true , Public:true }, "Concat", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.String")])], []),
      concatInternal
    );

    $.Method({Static: true , Public: true }, "EndsWith",
      new JSIL.MethodSignature("System.Boolean", ["System.String", "System.String"], [], $jsilcore),
      function (str, text) {
        return str.lastIndexOf(text) === str.length - text.length;
      }
    );

    $.Method({Static:true , Public:true }, "Format", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Array") /* AnyType[] */ ], []),
      function (format) {
        format = String(format);

        var regex = new RegExp("{([0-9]*)(?::([^}]*))?}", "g");
        var match = null;

        var values = Array.prototype.slice.call(arguments, 1);

        if ((values.length == 1) && JSIL.IsArray(values[0]))
          values = values[0];

        var matcher = function (match, index, valueFormat, offset, str) {
          index = parseInt(index);

          var value = values[index];

          if (valueFormat) {
            return JSIL.NumberToFormattedString(value, valueFormat);

          } else {

            if (typeof (value) === "boolean") {
              if (value)
                return "True";
              else
                return "False";
            } else {
              return String(value);
            }
          }
        };

        return format.replace(regex, matcher);
      }
    );

    $.Method({Static:true, Public:true }, "IndexOfAny", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [System.Array.Of($jsilcore.System.Char), $jsilcore.TypeRef("System.Int32")], []),
      function (str, chars) {
        var result = null;
        for (var i = 0; i < chars.length; i++) {
          var index = str.indexOf(chars[i]);
          if ((result === null) || (index < result))
            result = index;
        }

        if (result === null)
          return -1;
        else
          return result;
      }
    );

    $.Method({Static:true , Public:true }, "IsNullOrEmpty", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String")], []),
      function (str) {
        if (str === null)
          return true;
        else if (typeof (str) === "undefined")
          return true;
        else if (str.length === 0)
          return true;

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "IsNullOrWhiteSpace", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String")], []),
      function (str) {
        if (str === null)
          return true;
        else if (typeof (str) === "undefined")
          return true;
        else if (str.length === 0)
          return true;
        else if (str.trim().length === 0)
          return true;

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "LastIndexOfAny", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [System.Array.Of($jsilcore.System.Char), $jsilcore.TypeRef("System.Int32")], []),
      function (str, chars) {
        var result = null;
        for (var i = 0; i < chars.length; i++) {
          var index = str.lastIndexOf(chars[i]);
          if ((result === null) || (index > result))
            result = index;
        }

        if (result === null)
          return -1;
        else
          return result;
      }
    );

    $.Method({Static: true , Public: true }, "Remove",
      new JSIL.MethodSignature($.String, [$.String, $.Int32, $.Int32], [], $jsilcore),
      function (str, start, count) {
        return str.substr(0, start) + str.substr(start + count);
      }
    );

    $.Method({Static: true , Public: true }, "Replace",
      new JSIL.MethodSignature("System.String", ["System.String", "System.String", "System.String"], [], $jsilcore),
      function (str, oldText, newText) {
        return str.split(oldText).join(newText);
      }
    );

    $.Method({Static: true , Public: true }, "StartsWith",
      new JSIL.MethodSignature("System.Boolean", ["System.String", "System.String"], [], $jsilcore),
      function (str, text) {
        return str.indexOf(text) === 0;
      }
    );

    var makePadding = function (ch, count) {
      var padding = ch;
      for (var i = 1; i < count; i++) {
        padding += ch;
      }

      return padding;
    };

    $.Method({Static: true , Public: true }, "PadLeft",
      new JSIL.MethodSignature("System.String", ["System.String", "System.Int32", "System.Char"], [], $jsilcore),
      function (str, length, ch) {
        var extraChars = length - str.length;
        if (extraChars <= 0)
          return str;

        return makePadding(ch, extraChars) + str;
      }
    );

    $.Method({Static: true , Public: true }, "PadRight",
      new JSIL.MethodSignature("System.String", ["System.String", "System.Int32", "System.Char"], [], $jsilcore),
      function (str, length, ch) {
        var extraChars = length - str.length;
        if (extraChars <= 0)
          return str;

        return str + makePadding(ch, extraChars);
      }
    );

    $.Method({Static: true , Public: true }, "CopyTo",
      new JSIL.MethodSignature(null, ["System.String"], [], $jsilcore),
      function (str, sourceIndex, destination, destinationIndex, count) {
        if (count > 0) {
          for (var i = 0; i < count; i++)
            destination[destinationIndex + i] = str[sourceIndex + i];
        }
      }
    );
  }
);

JSIL.MakeClass("System.Object", "System.String", true, [], function ($) {
  $.__IsNativeType__ = true;

  $.Constant({Static: true , Public: true }, "Empty", "");
});

JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

$jsilcore.$GetInvocationList = function (delegate) {
    if (delegate === null) {
      return [ ];
    } else if (typeof (delegate.GetInvocationList) === "function") {
      return delegate.GetInvocationList();
    } else if (typeof (delegate) === "function") {
      return [ delegate ];
    } else {
      return null;
    }
};
$jsilcore.$Combine = function (lhs, rhs) {
  if (rhs === null) {
    return lhs;
  } else if (lhs === null) {
    return rhs;
  }

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  newList.push.apply(newList, $jsilcore.$GetInvocationList(rhs));
  var result = JSIL.MulticastDelegate.New(newList);
  return result;
};
$jsilcore.$Remove = function (lhs, rhs) {
  if (rhs === null)
    return lhs;
  if (lhs === null)
    return null;

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  var rhsList = $jsilcore.$GetInvocationList(rhs);

  for (var i = 0; i < rhsList.length; i++) {
    var needle = rhsList[i];

    __inner:
    for (var j = 0; j < newList.length; j++) {
      var haystack = newList[j];
      if ((haystack.__method__ === needle.__method__) &&
          (haystack.__object__ === needle.__object__)
      ) {
        newList.splice(j, 1);
        break __inner;
      }
    }
  }

  if (newList.length == 0)
    return null;
  else if (newList.length == 1)
    return newList[0];
  else
    return JSIL.MulticastDelegate.New(newList);
};

JSIL.ImplementExternals("System.Delegate", function ($) {
  var tDelegate = $jsilcore.TypeRef("System.Delegate");

  $.RawMethod(false, "Invoke", function () {
    return this.__method__.apply(this.__object__, arguments);
  });

  $.Method({Static:false, Public:true }, "GetInvocationList", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [tDelegate]), [], [])), 
    function GetInvocationList () {
      return [ this ];
    }
  );

  $.Method({Static:true , Public:true }, "CreateDelegate", 
    (new JSIL.MethodSignature(tDelegate, [
          $jsilcore.TypeRef("System.Type"), $.Object, 
          $jsilcore.TypeRef("System.Reflection.MethodInfo")
        ], [])), 
    function CreateDelegate (delegateType, firstArgument, method) {
      var isStatic = method._descriptor.Static;
      var key = method._data.mangledName || method._descriptor.EscapedName;
      var publicInterface = method._typeObject.__PublicInterface__;
      var context = isStatic ? publicInterface : publicInterface.prototype;
      var impl = context[key];

      if (typeof (impl) !== "function") {
        JSIL.Host.error(new Error("Failed to bind delegate: Method '" + key + "' not found in context"));
      }

      var delegatePublicInterface = delegateType.__PublicInterface__;

      if (typeof (delegatePublicInterface.New) !== "function") {
        JSIL.Host.error(new Error("Invalid delegate type"));
      }

      return delegatePublicInterface.New(firstArgument, impl);
    }
  );  

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    var lhsInvocationList = JSIL.Delegate.GetInvocationList(lhs);
    var rhsInvocationList = JSIL.Delegate.GetInvocationList(rhs);

    if (lhsInvocationList === rhsInvocationList)
      return true;

    if (!JSIL.IsArray(lhsInvocationList))
      return false;
    if (!JSIL.IsArray(rhsInvocationList))
      return false;

    if (lhsInvocationList.length != rhsInvocationList.length)
      return false;

    for (var i = 0, l = lhsInvocationList.length; i < l; i++) {
      if (lhsInvocationList[i] !== rhsInvocationList[i])
        return false;
    }

    return true;
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [tDelegate, tDelegate], [])), 
    equalsImpl
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [tDelegate, tDelegate], [])), 
    function op_Inequality (d1, d2) {
      return !equalsImpl(d1, d2);
    }
  );

  $.Method({Static:true , Public:true }, "Combine", 
    (new JSIL.MethodSignature(tDelegate, [tDelegate, tDelegate], [])), 
    $jsilcore.$Combine
  );

  $.Method({Static:true , Public:true }, "Remove", 
    (new JSIL.MethodSignature(tDelegate, [tDelegate, tDelegate], [])), 
    $jsilcore.$Remove
  );
});

JSIL.ImplementExternals("System.MulticastDelegate", function ($) {
  $.RawMethod(false, "Invoke", function () {
    return this.apply(null, arguments);
  });

  $.Method({Static:false, Public:true }, "GetInvocationList", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Delegate")]), [], [])), 
    function GetInvocationList () {
      return this.delegates;
    }
  );
});

JSIL.MakeClass("System.Object", "System.Delegate", true, []);
JSIL.MakeClass("System.Object", "System.MulticastDelegate", true, []);

JSIL.MulticastDelegate.New = function (delegates) {
  var invoker = function () {
    var result;
    for (var i = 0, l = this.length; i < l; i++) {
      var d = this[i];
      result = d.apply(null, arguments);
    }
    return result;
  };

  var result = invoker.bind(delegates);
  result.delegates = delegates;
  result.__proto__ = System.MulticastDelegate.prototype;
  Object.seal(result);
  return result;
};

JSIL.MakeDelegate("System.Action", true, []);
JSIL.MakeDelegate("System.Action`1", true, ["T"]);
JSIL.MakeDelegate("System.Action`2", true, ["T1", "T2"]);
JSIL.MakeDelegate("System.Action`3", true, ["T1", "T2", "T3"]);

JSIL.MakeDelegate("System.Func`1", true, ["TResult"]);
JSIL.MakeDelegate("System.Func`2", true, ["T", "TResult"]);
JSIL.MakeDelegate("System.Func`3", true, ["T1", "T2", "TResult"]);
JSIL.MakeDelegate("System.Func`4", true, ["T1", "T2", "T3", "TResult"]);

JSIL.ImplementExternals(
  "System.Exception", function ($) {
    var mscorlib = JSIL.GetCorlib();

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [], [])), 
      function _ctor () {
        this._message = null;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.String], [])), 
      function _ctor (message) {
        this._message = message;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.String, mscorlib.TypeRef("System.Exception")], [])), 
      function _ctor (message, innerException) {
        this._message = message;
        this._innerException = innerException;
      }
    );

    $.Method({Static:false, Public:true }, "get_InnerException", 
      (new JSIL.MethodSignature(mscorlib.TypeRef("System.Exception"), [], [])), 
      function get_InnerException () {
        return this._innerException;
      }
    );

    $.Method({Static: false, Public: true }, "get_Message",
      new JSIL.MethodSignature($.String, []),
      function () {
        if ((typeof (this._message) === "undefined") || (this._message === null))
          return System.String.Format("Exception of type '{0}' was thrown.", JSIL.GetTypeName(this));
        else
          return this._message;
      }
    );

    $.Method({Static: false, Public: true }, "toString",
      new JSIL.MethodSignature($.String, []),
      function () {
        var message = this.Message;
        return System.String.Format("{0}: {1}", JSIL.GetTypeName(this), message);
      }
    );
  }
);

JSIL.ImplementExternals(
  "System.SystemException", function ($) {
    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [], []),
      function () {
        System.Exception.prototype._ctor.call(this);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

JSIL.ImplementExternals(
  "System.InvalidCastException", function ($) {  
    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

JSIL.ImplementExternals(
  "System.InvalidOperationException", function ($) {  
    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

JSIL.ImplementExternals(
  "System.IO.FileNotFoundException", function ($) {  
    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.String, $.String], [])), 
      function _ctor (message, fileName) {
        System.Exception.prototype._ctor.call(this, message);
        this._fileName = fileName;
      }
    );
  }
);

JSIL.ImplementExternals(
  "System.FormatException", function ($) {  
    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

JSIL.MakeClass("System.Object", "System.Exception", true, [], function ($) {
  $.Property({Public: true , Static: false, Virtual: true }, "Message");
  $.Property({Public: true , Static: false}, "InnerException");
});

JSIL.MakeClass("System.Exception", "System.FormatException", true);
JSIL.MakeClass("System.Exception", "System.InvalidCastException", true);
JSIL.MakeClass("System.Exception", "System.InvalidOperationException", true);
JSIL.MakeClass("System.Exception", "System.IO.FileNotFoundException", true);

JSIL.ImplementExternals("System.Console", function ($) {
  $.RawMethod(true, "WriteLine", function () {
    var text = "";
    if (arguments.length > 0)
      text = System.String.Format.apply(System.String, arguments);

    JSIL.Host.logWriteLine(text);
  });

  $.RawMethod(true, "Write", function () {
    var text = "";
    if (arguments.length > 0)
      text = System.String.Format.apply(System.String, arguments);

    JSIL.Host.logWrite(text);
  });
});

JSIL.ImplementExternals(
  "System.Diagnostics.Debug", function ($) {
    $.Method({Static:true , Public:true }, "WriteLine", 
      (new JSIL.MethodSignature(null, [$.String], [])), 
      function WriteLine (message) {
        JSIL.Host.logWriteLine(message);
      }
    );

    $.Method({Static:true , Public:true }, "Write", 
      (new JSIL.MethodSignature(null, [$.String], [])), 
      function Write (message) {
        JSIL.Host.logWrite(message);
      }
    );
  }
);

JSIL.MakeStaticClass("System.Console", true, [], function ($) {
});

JSIL.SplitString = function (str, separators) {
  if (separators.length > 1)
    throw new Error("Split cannot handle more than one separator");

  return str.split(separators[0]);
};

JSIL.JoinStrings = function (separator, strings) {
  return strings.join(separator);
};

JSIL.ConcatString = function (/* ...values */) {
  var result = String(arguments[0]);

  for (var i = 1, l = arguments.length; i < l; i++) {
    var arg = arguments[i];
    if (typeof (arg) === "string")
      result += arg;
    else
      result += String(arg);
  }

  return result;
};

JSIL.MakeClass("System.Object", "JSIL.ArrayEnumerator", true, ["T"], function ($) {
  $.RawMethod(false, "__CopyMembers__", 
    function ArrayEnumerator_CopyMembers (source, target) {
      target._array = source._array;
      target._length = source._length;
      target._index = source._index;
    }
  );

  $.Method({Public: true , Static: false}, ".ctor", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32]),
    function (array, startPosition) {
      this._array = array;
      this._length = array.length;
      this._index = startPosition;
    }
  );

  $.Method({Public: true , Static: false}, "Reset", 
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._array === null)
        throw new Error("Enumerator is disposed or not initialized");

      this._index = -1;
    }
  );
  $.Method({Public: true , Static: false}, "MoveNext", 
    new JSIL.MethodSignature(System.Boolean, []),
    function () {
      return (++this._index < this._length);
    }
  );
  $.Method({Public: true , Static: false}, "Dispose", 
    new JSIL.MethodSignature(null, []),
    function () {
      this._array = null;
      this._index = 0;
      this._length = -1;
    }
  );
  $.Method({Public: true , Static: false}, "get_Current", 
    new JSIL.MethodSignature(System.Object, []),
    function () {
      return this._array[this._index];
    }
  );

  $.Property({Public: true , Static: false, Virtual: true }, "Current");

  $.ImplementInterfaces(
    System.IDisposable, System.Collections.IEnumerator,
    $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "JSIL.ArrayEnumerator")])
  );
});

JSIL.ImplementExternals(
  "System.Threading.Thread", function ($) {
    $.Method({Static:true , Public:true }, ".cctor2", 
      (new JSIL.MethodSignature(null, [], [])), 
      function () {
        // This type already has a cctor, so we add a second one.
        System.Threading.Thread._currentThread = JSIL.CreateInstanceOfType(
          System.Threading.Thread.__Type__,
          null
        );
      }
    );

    $.Method({Static:true , Public:true }, "get_CurrentThread", 
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Thread"), [], [])), 
      function get_CurrentThread () {
        return System.Threading.Thread._currentThread;
      }
    );

    $.Method({Static:false, Public:true }, "get_ManagedThreadId", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_ManagedThreadId () {
        return 0;
      }
    );
  }
);

JSIL.MakeClass("System.Object", "System.Threading.Thread", true, [], function ($) {
  $.Field({Public: false, Static: true}, "_currentThread", $.Type, function ($) { return null; });

  $.ExternalMethod(
    {Public: true , Static: true }, "get_CurrentThread",
    new JSIL.MethodSignature($.Type, [])
  );
  $.ExternalMethod(
    {Public: true , Static: true }, "get_ManagedThreadId",
    new JSIL.MethodSignature(System.Int32, [])
  );

  $.Property({Public: true , Static: true }, "CurrentThread");
  $.Property({Public: true , Static: true }, "ManagedThreadId");
});

$jsilcore.InitResizableArray = function (target, elementType, initialSize) {
  target._items = new Array();
};

$jsilcore.$ListExternals = function ($, T, type) {
  var mscorlib = JSIL.GetCorlib();

  if ((typeof (T) === "undefined") || (T === null)) {
    T = new JSIL.GenericParameter("T", "System.Collections.Generic.List`1");
  }

  var getT;

  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      getT = function () { return System.Object; }
      break;
    default:
      getT = function (self) { return self.T; }
      break;
  }

  var indexOfImpl = function List_IndexOf (value) {
    for (var i = 0, l = this._size; i < l; i++) {
      if (JSIL.ObjectEquals(this._items[i], value))
        return i;
    }

    return -1;
  };

  var findIndexImpl = function List_FindIndex (predicate) {
    for (var i = 0, l = this._size; i < l; i++) {
      if (predicate(this._items[i]))
        return i;
    }

    return -1;
  };

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      $jsilcore.InitResizableArray(this, getT(this), 16);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32")], []),
    function (size) {
      $jsilcore.InitResizableArray(this, getT(this), size);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")])], []),
    function (values) {
      this._items = JSIL.EnumerableToArray(values);
      this._capacity = this._items.length;
      this._size = this._items.length;
    }
  );

  var addImpl = function (item) {
    if (this._size >= this._items.length) {
      this._items.push(item);
    } else {
      this._items[this._size] = item;
    }
    this._size += 1;

    if (typeof (this.$OnItemAdded) === "function")
      this.$OnItemAdded(item);

    return this._size;
  };

  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      $.Method({Static:false, Public:true }, "Add", 
        new JSIL.MethodSignature($.Int32, [T], []),
        addImpl
      );
      break;
    default:
      $.Method({Static:false, Public:true }, "Add", 
        new JSIL.MethodSignature(null, [T], []),
        addImpl
      );
      break;
  }

  $.Method({Static:false, Public:true }, "AddRange", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IEnumerable`1", [T])], []),
    function (items) {
      var e = JSIL.GetEnumerator(items);
      try {
        while (e.IEnumerator_MoveNext())
          this.Add(e.IEnumerator_Current);
      } finally {
        e.IDisposable_Dispose();
      }
    }
  );

  $.Method({Static:false, Public:true }, "AsReadOnly", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.ObjectModel.ReadOnlyCollection`1", [T]), [], []),
    function () {
      // FIXME
      if (typeof (this.tReadOnlyCollection) === "undefined") {
        this.tReadOnlyCollection = System.Collections.ObjectModel.ReadOnlyCollection$b1.Of(this.T).__Type__;
      }

      return JSIL.CreateInstanceOfType(this.tReadOnlyCollection , "$listCtor", [this]);
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Contains", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [T], []),
    function List_Contains (value) {
      return this.IndexOf(value) >= 0;
    }
  );

  $.Method({Static:false, Public:true }, "Exists", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function List_Exists (predicate) {
      return this.FindIndex(predicate) >= 0;
    }
  );

  $.Method({Static:false, Public:true }, "Find", 
    new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.List`1"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function List_Find (predicate) {
      var index = this.FindIndex(predicate);
      if (index >= 0)
        return this._items[index];
      else
        return JSIL.DefaultValue(this.T);
    }
  );

  $.Method({Static:false, Public:true }, "FindAll", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.List`1", [T]), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      var thisType = this.GetType();

      // Manually initialize the result since we don't want to hassle with overloaded ctors
      var result = JSIL.CreateInstanceOfType(thisType, null);
      result._items = [];

      for (var i = 0; i < this._size; i++) {
        var item = this._items[i];

        if (predicate(item))
          result._items.push(item);
      }

      result._capacity = result._size = result._items.length;
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "FindIndex", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [mscorlib.TypeRef("System.Predicate`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")])], []),
    findIndexImpl
  );

  $.Method({Static:false, Public:true }, "get_Capacity", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [], []),
    function () {
      return this._items.length;
    }
  );

  $.Method({Static:false, Public:true }, "get_Count", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [], []),
    function () {
      return this._size;
    }
  );

  var rangeCheckImpl = function (index, size) {
    return (index >= 0) && (size > index);
  }

  $.Method({Static:false, Public:true }, "get_Item", 
    new JSIL.MethodSignature(T, [mscorlib.TypeRef("System.Int32")], []), 
    function (index) {
      if (rangeCheckImpl(index, this._size))
        return this._items[index];
      else
        throw new System.ArgumentOutOfRangeException("index");
    }
  );

  $.Method({Static: false, Public: true }, "set_Item",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), T], []), 
    function (index, value) {
      if (rangeCheckImpl(index, this._size))
        this._items[index]=value;
      else
        throw new System.ArgumentOutOfRangeException("index");
    }
  );

  var getEnumeratorType = function (self) {
    if (self.$enumeratorType)
      return self.$enumeratorType;

    var T = getT(self);
    return self.$enumeratorType = System.Collections.Generic.List$b1_Enumerator.Of(T);
  };

  var getEnumeratorImpl = function () {
    var enumeratorType = getEnumeratorType(this);

    return new enumeratorType(this);
  };

  $.Method({Static:false, Public:true }, "IEnumerable_GetEnumerator", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], []),
    getEnumeratorImpl
  );

  $.Method({Static:false, Public:true }, "IEnumerable$b1_GetEnumerator",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []),
    getEnumeratorImpl
  );

  $.RawMethod(false, "$GetEnumerator", getEnumeratorImpl);

  switch (type) {
    case "ArrayList":
      $.Method({Static:false, Public:true }, "GetEnumerator", 
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], []),
        getEnumeratorImpl
      );
      break;
    case "List":
      $.Method({Static:false, Public:true }, "GetEnumerator", 
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.List`1/Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]), [], [])), 
        getEnumeratorImpl
      );
      break;
    default:
      $.Method({Static:false, Public:true }, "GetEnumerator",
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []),
        getEnumeratorImpl
      );
      break;
  }

  $.Method({Static:false, Public:true }, "Insert", 
    (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")], [])), 
    function Insert (index, item) {
      this._items.splice(index, 0, item);
      this._size += 1;
    }
  );

  $.Method({Static:false, Public:true }, "IndexOf", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [T], []),
    indexOfImpl
  );

  var removeImpl = function (item) {
    var index = this._items.indexOf(item);
    if (index === -1)
      return false;

    return this.RemoveAt(index);
  };

  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      $.Method({Static:false, Public:true }, "Remove", 
        new JSIL.MethodSignature(null, [T], []),
        removeImpl
      );
      break;
    default:
      $.Method({Static:false, Public:true }, "Remove", 
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [T], []),
        removeImpl
      );
      break;
  }

  $.Method({Static:false, Public:true }, "RemoveAll", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      var result = 0;

      for (var i = 0; i < this._size; i++) {
        var item = this._items[i];

        if (predicate(item)) {
          this._items.splice(i, 1);
          i -= 1;
          this._size -= 1;
          result += 1;
        }
      }

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "RemoveAt", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32")], []),
    function (index) {
      this._items.splice(index, 1);
      this._size -= 1;
      return true;
    }
  );

  $.Method({Static:false, Public:true }, "Sort", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      this._items.sort();
    }
  );

  $.Method({Static:false, Public:true }, "Sort", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Comparison`1", [T])], []),
    function (comparison) {
      this._items.sort(comparison);
    }
  );

  $.Method({Static:false, Public:true }, "Sort", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.IComparer")], [])), 
    function Sort (comparer) {
      this._items.sort(function (lhs, rhs) {
        return comparer.Compare(lhs, rhs);
      });
    }
  );

  $.Method({Static:false, Public:true }, "Sort", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IComparer`1", [T])], [])), 
    function Sort (comparer) {
      this._items.sort(function (lhs, rhs) {
        return comparer.Compare(lhs, rhs);
      });
    }
  );

  $.Method({Static:false, Public:true }, "ToArray", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [T]), [], []),
    function () {
      return Array.prototype.slice.call(this._items, 0, this._size);
    }
  );

  $.Method({Static:false, Public:true }, "TrueForAll", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      for (var i = 0; i < this._size; i++) {
        var item = this._items[i];

        if (!predicate(item))
          return false;
      }

      return true;
    }
  );
};

JSIL.ImplementExternals("System.Collections.Generic.List`1", function ($) {
  $jsilcore.$ListExternals($, null, "List");

  $.Method({ Static: false, Public: true }, "CopyTo",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]), $.Int32], []),
    function (array, arrayindex) {
      if (arrayindex != 0) {
          throw new Error("List<T>.CopyTo not supported for non-zero indexes");
      }

      JSIL.Array.ShallowCopy(array, this._items);
    }
  );

  $.Method({Static:false, Public:true }, "ICollection$b1_get_IsReadOnly",
    new JSIL.MethodSignature($.Boolean, [], []),
    function () {
      return false;
    }
  );

});

$jsilcore.$ArrayListExternals = function ($) {
  $jsilcore.$ListExternals($, $.Object, "ArrayList");

  var mscorlib = JSIL.GetCorlib();
  var toArrayImpl = function () {
    return Array.prototype.slice.call(this._items, 0, this._size);
  };

  $.Method({Static:false, Public:true }, "ToArray", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Array", [mscorlib.TypeRef("System.Object")]), [], []),
    toArrayImpl
  );

  $.Method({Static:false, Public:true }, "ToArray", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Array"), [mscorlib.TypeRef("System.Type")], []),
    toArrayImpl
  );
};

// Lazy way of sharing method implementations between ArrayList, Collection<T> and List<T>.
JSIL.ImplementExternals("System.Collections.ArrayList", $jsilcore.$ArrayListExternals);

$jsilcore.$CollectionExternals = function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.ObjectModel.Collection`1");
  $jsilcore.$ListExternals($, T, "List");

  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      $jsilcore.InitResizableArray(this, this.T, 16);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IList`1", [T])], []),
    function (list) {
      this._items = JSIL.EnumerableToArray(list);
      this._capacity = this._size = this._items.length;
    }
  );
};

JSIL.ImplementExternals("System.Collections.ObjectModel.Collection`1", $jsilcore.$CollectionExternals);

$jsilcore.$ReadOnlyCollectionExternals = function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.ObjectModel.ReadOnlyCollection`1");
  $jsilcore.$ListExternals($, T, "ReadOnlyCollection");

  var mscorlib = JSIL.GetCorlib();

  var listCtor = function (list) {
    this._list = list;

    Object.defineProperty(this, "_items", {
      get: function () {
        return list._items;
      }
    });

    Object.defineProperty(this, "_size", {
      get: function () {
        return list._size;
      }
    });
  };

  $.RawMethod(false, "$listCtor", listCtor);

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IList`1", [T])], []),
    listCtor
  );

  $.SetValue("Add", null);
  $.SetValue("Clear", null);
  $.SetValue("Remove", null);
  $.SetValue("RemoveAt", null);
  $.SetValue("RemoveAll", null);
  $.SetValue("Sort", null);
};

JSIL.ImplementExternals("System.Collections.ObjectModel.ReadOnlyCollection`1", $jsilcore.$ReadOnlyCollectionExternals);

JSIL.ImplementExternals("System.Collections.Generic.Stack`1", function ($) {
  var system = JSIL.GetAssembly("System", true);

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      $jsilcore.InitResizableArray(this, this.T, 16);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (capacity) {
      $jsilcore.InitResizableArray(this, this.T, capacity);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Clear () {
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      return this._size;
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature(system.TypeRef("System.Collections.Generic.Stack`1/Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")]), [], [])), 
    function GetEnumerator () {
      return this.$GetEnumerator();
    }
  );

  $.Method({Static:false, Public:true }, "Peek", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1"), [], [])), 
    function Peek () {
      if (this._size <= 0)
        throw new System.InvalidOperationException("Stack is empty");

      return this._items[this._size - 1];
    }
  );

  $.Method({Static:false, Public:true }, "Pop", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1"), [], [])), 
    function Pop () {
      var result = this._items.pop();
      this._size -= 1;

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "Push", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")], [])), 
    function Push (item) {
      this._items.push(item)
      this._size += 1;
    }
  );

});

JSIL.ImplementExternals("System.Collections.Generic.Queue`1", function ($) {
  var system = JSIL.GetAssembly("System", true);

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      $jsilcore.InitResizableArray(this, this.T, 16);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (capacity) {
      $jsilcore.InitResizableArray(this, this.T, capacity);
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Clear () {
      this._size = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Dequeue", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1"), [], [])), 
    function Dequeue () {
      var result = this._items.shift();
      this._size -= 1;
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "Enqueue", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1")], [])), 
    function Enqueue (item) {
      this._items.push(item);
      this._size += 1;
    }
  );

  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      return this._size;
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature(system.TypeRef("System.Collections.Generic.Queue`1/Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1")]), [], [])), 
    function GetEnumerator () {
      return this.$GetEnumerator();
    }
  );

});

JSIL.MakeClass("System.Object", "JSIL.EnumerableArray", true, [], function ($) {
  $.Method({Public: true , Static: false}, ".ctor", 
    new JSIL.MethodSignature(null, [System.Array]),
    function (array) {
      this.array = array;
    }
  );

  var tEnumerator = [null];

  $.Method({Public: true , Static: false}, "GetEnumerator", 
    new JSIL.MethodSignature(System.Collections.IEnumerator$b1, []),
    function () {
      if (tEnumerator[0] === null) {
        tEnumerator[0] = JSIL.ArrayEnumerator.Of(System.Object);
      }

      return new (tEnumerator[0])(this.array, -1);
    }
  );

  $.ImplementInterfaces(
    System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
  );
});

JSIL.MakeClass("System.Object", "System.Collections.ArrayList", true, [], function ($) {
  $.Property({Public: true , Static: false}, "Count");

  $.ImplementInterfaces(
    "System.Collections.IEnumerable"
  );
});

JSIL.MakeClass("System.Object", "System.Collections.Generic.List`1", true, ["T"], function ($) {
  $.Property({Public: true , Static: false}, "Count");
  $.Property({Public: false, Static: false}, "ICollection`1.IsReadOnly");

  $.ImplementInterfaces(
    $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    "System.Collections.IEnumerable",
    $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    $jsilcore.TypeRef("System.Collections.Generic.IList`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")])
  );
});

JSIL.MakeClass("System.Object", "System.Collections.Generic.Stack`1", true, ["T"], function ($) {
	$.Property({Public: true , Static: false}, "Count");

	$.ImplementInterfaces(
		$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")]), 
    "System.Collections.IEnumerable"
	);
});

// TODO: This type is actually a struct in the CLR
JSIL.MakeClass($jsilcore.TypeRef("JSIL.ArrayEnumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1/Enumerator")]), "System.Collections.Generic.List`1/Enumerator", true, ["T"], function ($) {
  $.Field({Public: false, Static: false}, "_array", Array, function ($) { return null; });
  $.Field({Public: false, Static: false}, "_length", Number, function ($) { return 0; });
  $.Field({Public: false, Static: false}, "_index", Number, function ($) { return -1; });

  $.Method({Public: true, Static: false}, ".ctor", 
    new JSIL.MethodSignature(null, ["System.Collections.Generic.List`1"]),
    function (list) {
      this._array = list._items;
      this._length = list._size;
    }
  );

  $.ImplementInterfaces(
    $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1/Enumerator")])
  );
});

JSIL.ImplementExternals(
  "System.Threading.Interlocked", function ($) {
    var cmpxchg = function (targetRef, value, comparand) {
      var currentValue = targetRef.value;

      if (currentValue === comparand)
        targetRef.value = value;

      return currentValue;
    };

    $.Method({Public: true , Static: true }, "CompareExchange", 
      new JSIL.MethodSignature("!!0", [JSIL.Reference.Of("!!0"), "!!0", "!!0"], ["T"]),
      function (T, targetRef, value, comparand) {
        return cmpxchg(targetRef, value, comparand);
      }
    );

    $.Method({Static:true , Public:true }, "CompareExchange", 
      (new JSIL.MethodSignature($.Int32, [
            $jsilcore.TypeRef("JSIL.Reference", [$.Int32]), $.Int32, 
            $.Int32
          ], [])), 
      function CompareExchange (/* ref */ location1, value, comparand) {
        return cmpxchg(location1, value, comparand);
      }
    );
  }
);

JSIL.ImplementExternals("System.Threading.Monitor", function ($) {
  var enterImpl = function (obj) {
    var current = (obj.__LockCount__ || 0);
    if (current >= 1)
      JSIL.Host.warning("Warning: lock recursion ", obj);

    obj.__LockCount__ = current + 1;

    return true;
  };

  $.Method({Static:true , Public:true }, "Enter", 
    (new JSIL.MethodSignature(null, [$.Object], [])), 
    function Enter (obj) {
      enterImpl(obj);
    }
  );

  $.Method({Static:true , Public:true }, "Enter", 
    (new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("JSIL.Reference", [$.Boolean])], [])), 
    function Enter (obj, /* ref */ lockTaken) {
      lockTaken.value = enterImpl(obj);
    }
  );

  $.Method({Static:true , Public:true }, "Exit", 
    (new JSIL.MethodSignature(null, [$.Object], [])), 
    function Exit (obj) {
      var current = (obj.__LockCount__ || 0);
      if (current <= 0)
        JSIL.Host.warning("Warning: unlocking an object that is not locked ", obj);

      obj.__LockCount__ = current - 1;
    }
  );

});

JSIL.MakeStaticClass("System.Threading.Interlocked", true, [], function ($) {
  $.ExternalMethod({Public: true , Static: true }, "CompareExchange", 
    new JSIL.MethodSignature("!!0", [JSIL.Reference.Of("!!0"), "!!0", "!!0"], ["T"])
  );
});
JSIL.MakeStaticClass("System.Threading.Monitor", true, []);

JSIL.ImplementExternals("System.Random", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (Seed) {
      JSIL.Host.warning("Cannot seed the JS random number generator.");
    }
  );

  $.Method({Static:false, Public:true }, "Next", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function Next () {
      return Math.floor(Math.random() * Int32.MaxValue);
    }
  );

  $.Method({Static:false, Public:true }, "Next", 
    (new JSIL.MethodSignature($.Int32, [$.Int32, $.Int32], [])), 
    function Next (minValue, maxValue) {
      return Math.floor(Math.random() * (maxValue - minValue)) + minValue;
    }
  );

  $.Method({Static:false, Public:true }, "Next", 
    (new JSIL.MethodSignature($.Int32, [$.Int32], [])), 
    function Next (maxValue) {
      return Math.floor(Math.random() * maxValue);
    }
  );

  $.Method({Static:false, Public:true }, "NextDouble", 
    (new JSIL.MethodSignature($.Double, [], [])), 
    Math.random
  );
});

JSIL.MakeClass("System.Object", "System.Random", true, [], function ($) {
});

JSIL.$MathSign = function (value) {
  if (value > 0)
    return 1;
  else if (value < 0)
    return -1;
  else
    return 0;
};

JSIL.ImplementExternals("System.Math", function ($) {
  $.RawMethod(true, "Max", Math.max);
  $.RawMethod(true, "Min", Math.min);
  $.RawMethod(true, "Exp", Math.exp);

  $.Method({Static:true , Public:true }, "Round", 
    (new JSIL.MethodSignature($.Double, [$.Double, $.Int32], [])), 
    function Round (value, digits) {
      var multiplier = Math.pow(10, digits);
      var result = Math.round(value * multiplier) / multiplier;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "Atan2", 
    (new JSIL.MethodSignature($.Double, [$.Double, $.Double], [])), 
    Math.atan2
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.SByte], [])), 
    JSIL.$MathSign
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.Int16], [])), 
    JSIL.$MathSign
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.Int32], [])), 
    JSIL.$MathSign
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.Int64], [])), 
    JSIL.$MathSign
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.Single], [])), 
    JSIL.$MathSign
  );

  $.Method({Static:true , Public:true }, "Sign", 
    (new JSIL.MethodSignature($.Int32, [$.Double], [])), 
    JSIL.$MathSign
  );
});

JSIL.MakeStaticClass("System.Math", true, function ($) {
});

JSIL.MakeStruct("System.ValueType", "System.Decimal", true, [], function ($) {
  var mscorlib = JSIL.GetCorlib();

  var ctorImpl = function (value) {
    this.value = value.valueOf();
  };

  var decimalToNumber = function (decimal) {
    return decimal.valueOf();
  };

  var numberToDecimal = function (value) {
    var result = JSIL.CreateInstanceOfType($.Type, null);
    result.value = value.valueOf();
    return result;
  };

  $.RawMethod(false, "valueOf", function () {
    return this.value;
  });

  $.Method({Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function (format) {
      return this.value.toString();
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32")], [])),
    ctorImpl
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.UInt32")], [])),
    ctorImpl
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int64")], [])),
    ctorImpl
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.UInt64")], [])),
    ctorImpl
  );

  $.Method({Static:true , Public:true }, "op_Addition", 
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (lhs, rhs) {
      return numberToDecimal(decimalToNumber(lhs) + decimalToNumber(rhs));
    }
  );

  $.Method({Static:true , Public:true }, "op_Division", 
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (lhs, rhs) {
      return numberToDecimal(decimalToNumber(lhs) / decimalToNumber(rhs));
    }
  );

  $.Method({Static:true , Public:true }, "op_Multiply", 
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (lhs, rhs) {
      return numberToDecimal(decimalToNumber(lhs) * decimalToNumber(rhs));
    }
  );

  $.Method({Static:true , Public:true }, "op_Subtraction", 
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (lhs, rhs) {
      return numberToDecimal(decimalToNumber(lhs) - decimalToNumber(rhs));
    }
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Single")], [])),
    numberToDecimal
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Double")], [])),
    numberToDecimal
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Byte"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.SByte"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Int16"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.UInt16"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.UInt32"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Int64"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.UInt64"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Single"), [$.Type], [])),
    decimalToNumber
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Double"), [$.Type], [])),
    decimalToNumber
  );

  $.Field({Static: false, Public: false }, "value", mscorlib.TypeRef("System.Double"), function () {
    return 0;
  });
});

JSIL.ImplementExternals("System.Environment", function ($) {

  $.Method({Static:true , Public:true }, "GetFolderPath", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Environment/SpecialFolder")], [])), 
    function GetFolderPath (folder) {
      // FIXME
      return folder.name;
    }
  );

  $.Method({Static:true , Public:true }, "get_NewLine", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_NewLine () {
      // FIXME: Maybe this should just be \n?
      return "\r\n";
    }
  );

});

$jsilcore.fromCharCode = function fixedFromCharCode (codePt) {  
  // https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/String/fromCharCode
  if (codePt > 0xFFFF) {  
    codePt -= 0x10000;  
    return String.fromCharCode(0xD800 + (codePt >> 10), 0xDC00 + (codePt & 0x3FF));  
  } else {  
    return String.fromCharCode(codePt); 
  }  
};

$jsilcore.charCodeAt = function fixedCharCodeAt (str, idx) {  
  // https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/String/charCodeAt

  idx = idx || 0;  
  var code = str.charCodeAt(idx);  
  var hi, low;  

  if (0xD800 <= code && code <= 0xDBFF) { 
    // High surrogate (could change last hex to 0xDB7F to treat high private surrogates as single characters)  
    hi = code;
    low = str.charCodeAt(idx+1);  
    if (isNaN(low))
      throw new Error("High surrogate not followed by low surrogate");

    return ((hi - 0xD800) * 0x400) + (low - 0xDC00) + 0x10000;  
  }

  if (0xDC00 <= code && code <= 0xDFFF) { 
    // Low surrogate  
    // We return false to allow loops to skip this iteration since should have already handled high surrogate above in the previous iteration  
    return false;  
  }  

  return code;  
};

$jsilcore.makeByteReader = function (bytes, index, count) {
  var position = index || 0;
  var endpoint;

  if (count)
    endpoint = (position + count);
  else
    endpoint = (bytes.length - position);

  var result = {
    read: function () {
      if (position >= endpoint)
        return false;

      var nextByte = bytes[position];
      position += 1;
      return nextByte;
    }
  };

  Object.defineProperty(result, "eof", {
    get: function () {
      return (position >= endpoint);
    },
    configurable: true,
    enumerable: true
  });

  return result;
};

$jsilcore.makeCharacterReader = function (str) {
  var position = 0, length = str.length;
  var cca = $jsilcore.charCodeAt;

  var result = {
    read: function () {
      if (position >= length)
        return false;

      var nextChar = cca(str, position);
      position += 1;
      return nextChar;
    }
  };

  Object.defineProperty(result, "eof", {
    get: function () {
      return (position >= length);
    },
    configurable: true,
    enumerable: true
  });

  return result;
};

JSIL.ImplementExternals("System.Text.Encoding", function ($) {
  $.Method({Static:true , Public:true }, ".cctor2", 
    (new JSIL.MethodSignature(null, [], [])),
    function () {
      // This type already has a cctor so we add a second one.
      System.Text.Encoding.asciiEncoding = JSIL.CreateInstanceOfType(
        System.Text.ASCIIEncoding.__Type__, "$fromCharset", ["US-ASCII"]
      );

      System.Text.Encoding.utf8Encoding = JSIL.CreateInstanceOfType(
        System.Text.UTF8Encoding.__Type__, "$fromCharset", ["UTF-8"]
      );

      System.Text.Encoding.utf7Encoding = JSIL.CreateInstanceOfType(
        System.Text.UTF7Encoding.__Type__, "$fromCharset", ["UTF-7"]
      );

      System.Text.Encoding.unicodeEncoding = JSIL.CreateInstanceOfType(
        System.Text.UnicodeEncoding.__Type__, "$fromCharset", ["UTF-16"]
      );
    }
  );

  $.RawMethod(false, "$fromCharset", function (charset) {
    this._charset = charset;
    this.fallbackCharacter = "?";
  });

  $.RawMethod(false, "$makeWriter", function (outputBytes, outputIndex) {
    var i = outputIndex;
    var count = 0;

    if (JSIL.IsArray(outputBytes)) {
      return {
        write: function (byte) {
          if (i >= outputBytes.length)
            throw new Error("End of buffer");

          outputBytes[i] = byte;
          i++;
          count++;
        },
        getResult: function () {
          return count;
        }
      };
    } else {
      var resultBytes = new Array();
      return {
        write: function (byte) {
          resultBytes.push(byte);
        },
        getResult: function () {
          if (typeof (Uint8Array) !== "undefined")
            return new Uint8Array(resultBytes);
          else
            return resultBytes;
        }
      };
    }
  });

  $.RawMethod(false, "$fromCharCode", $jsilcore.fromCharCode);

  $.RawMethod(false, "$charCodeAt", $jsilcore.charCodeAt);

  $.RawMethod(false, "$makeCharacterReader", $jsilcore.makeCharacterReader);

  $.RawMethod(false, "$makeByteReader", $jsilcore.makeByteReader);

  $.RawMethod(false, "$encode", function Encoding_Encode_PureVirtual (string, outputBytes, outputIndex) {
    throw new Error("Not implemented");
  });

  $.RawMethod(false, "$decode", function Encoding_Decode_PureVirtual (bytes, index, count) {
    throw new Error("Not implemented");
  });

  $.RawMethod(false, "$charsToString", function (chars, index, count) {
    if (typeof (index) === "undefined")
      index = 0;
    if (typeof (count) === "undefined")
      count = chars.length;

    return JSIL.StringFromByteArray(chars, index, count);
  });

  $.RawMethod(false, "$stringToChars", function (string) {
    return Array.prototype.slice.call(string);
  });

  $.Method({Static:true , Public:true }, "get_ASCII", 
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      return System.Text.Encoding.asciiEncoding;
    }
  );

  $.Method({Static:true , Public:true }, "get_UTF8", 
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      return System.Text.Encoding.utf8Encoding;
    }
  );

  $.Method({Static:true , Public:true }, "get_UTF7", 
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      return System.Text.Encoding.utf7Encoding;
    }
  );

  $.Method({Static:true , Public:true }, "get_Unicode", 
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      return System.Text.Encoding.unicodeEncoding;
    }
  );

  $.Method({Static:false, Public:true }, "GetByteCount", 
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Char])], [])), 
    function GetByteCount (chars) {
      return this.$encode(this.$charsToString(chars)).length;
    }
  );

  $.Method({Static:false, Public:true }, "GetByteCount", 
    (new JSIL.MethodSignature($.Int32, [$.String], [])), 
    function GetByteCount (s) {
      return this.$encode(s).length;
    }
  );

  $.Method({Static:false, Public:true }, "GetByteCount", 
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32, 
          $.Int32
        ], [])), 
    function GetByteCount (chars, index, count) {
      return this.$encode(this.$charsToString(chars, index, count)).length;
    }
  );

  $.Method({Static:false, Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$jsilcore.TypeRef("System.Array", [$.Char])], [])), 
    function GetBytes (chars) {
      return this.$encode(this.$charsToString(chars));
    }
  );

  $.Method({Static:false, Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32, 
          $.Int32
        ], [])), 
    function GetBytes (chars, index, count) {
      return this.$encode(this.$charsToString(chars, index, count));
    }
  );

  $.Method({Static:false, Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32, 
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Byte]), 
          $.Int32
        ], [])), 
    function GetBytes (chars, charIndex, charCount, bytes, byteIndex) {
      return this.$encode(this.$charsToString(chars, index, count), bytes, byteIndex);
    }
  );

  $.Method({Static:false, Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], [])), 
    function GetBytes (s) {
      return this.$encode(s);
    }
  );

  $.Method({Static:false, Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($.Int32, [
          $.String, $.Int32, 
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Byte]), 
          $.Int32
        ], [])), 
    function GetBytes (s, charIndex, charCount, bytes, byteIndex) {
      return this.$encode(s.substr(charIndex, charCount), bytes, byteIndex);
    }
  );

  $.Method({Static:false, Public:true }, "GetCharCount", 
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function GetCharCount (bytes) {
      return this.$decode(bytes).length;
    }
  );

  $.Method({Static:false, Public:true }, "GetCharCount", 
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function GetCharCount (bytes, index, count) {
      return this.$decode(bytes, index, count).length;
    }
  );

  $.Method({Static:false, Public:true }, "GetChars", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function GetChars (bytes) {
      return this.$stringToChars(this.$decode(bytes));
    }
  );

  $.Method({Static:false, Public:true }, "GetChars", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function GetChars (bytes, index, count) {
      return this.$stringToChars(this.$decode(bytes, index, count));
    }
  );

  $.Method({Static:false, Public:true }, "GetChars", 
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Char]), 
          $.Int32
        ], [])), 
    function GetChars (bytes, byteIndex, byteCount, chars, charIndex) {
      throw new Error("Not implemented");
    }
  );

  $.Method({Static:false, Public:true }, "GetString", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function GetString (bytes) {
      return this.$decode(bytes);
    }
  );

  $.Method({Static:false, Public:true }, "GetString", 
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function GetString (bytes, index, count) {
      return this.$decode(bytes, index, count);
    }
  );
});

JSIL.MakeClass("System.Object", "System.Text.Encoding", true, [], function ($) {
  $.Property({Static:true , Public:true }, "ASCII");
  $.Property({Static:true , Public:true }, "UTF8");
  $.Property({Static:true , Public:true }, "UTF7");
  $.Property({Static:true , Public:true }, "Unicode");
});

JSIL.ImplementExternals("System.Text.ASCIIEncoding", function ($) {
  $.RawMethod(false, "$encode", function ASCIIEncoding_Encode (string, outputBytes, outputIndex) {
    var writer = this.$makeWriter(outputBytes, outputIndex);

    var fallbackCharacter = this.fallbackCharacter.charCodeAt(0);
    var reader = this.$makeCharacterReader(string), ch;

    while (!reader.eof) {
      ch = reader.read();

      if (ch === false)
        continue;
      else if (ch <= 127)
        writer.write(ch);
      else
        writer.write(fallbackCharacter);
    }

    return writer.getResult();
  });

  $.RawMethod(false, "$decode", function ASCIIEncoding_Decode (bytes, index, count) {
    var reader = this.$makeByteReader(bytes, index, count), byte;
    var result = "";

    while (!reader.eof) {
      byte = reader.read();

      if (byte === false)
        continue;
      else if (byte > 127)
        result += this.fallbackCharacter;
      else
        result += String.fromCharCode(byte);
    }

    return result;
  });
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.ASCIIEncoding", true, [], function ($) {
});

JSIL.ImplementExternals("System.Text.UTF8Encoding", function ($) {
  var UTF8ByteSwapNotAChar = 0xFFFE;
  var UTF8NotAChar         = 0xFFFF;

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.emitBOM = false;
      this.throwOnInvalid = false;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function _ctor (encoderShouldEmitUTF8Identifier) {
      this.emitBOM = encoderShouldEmitUTF8Identifier;
      this.throwOnInvalid = false;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Boolean, $.Boolean], [])), 
    function _ctor (encoderShouldEmitUTF8Identifier, throwOnInvalidBytes) {
      this.emitBOM = encoderShouldEmitUTF8Identifier;
      this.throwOnInvalid = throwOnInvalidBytes;
    }
  );

  $.RawMethod(false, "$encode", function UTF8Encoding_Encode (string, outputBytes, outputIndex) {
    // http://tidy.sourceforge.net/cgi-bin/lxr/source/src/utf8.c

    var writer = this.$makeWriter(outputBytes, outputIndex);
    var reader = this.$makeCharacterReader(string), ch;

    var hasError = false;

    while (!reader.eof) {
      ch = reader.read();

      if (ch === false)
        continue;

      if (ch <= 0x7F) {
        writer.write( ch );
      } else if (ch <= 0x7FF) {
        writer.write( 0xC0 | (ch >> 6) );
        writer.write( 0x80 | (ch & 0x3F) );
      } else if (ch <= 0xFFFF) {
        writer.write( 0xE0 | (ch >> 12) );
        writer.write( 0x80 | ((ch >> 6) & 0x3F) );
        writer.write( 0x80 | (ch & 0x3F) );
      } else if (ch <= 0x1FFFF) {
        writer.write( 0xF0 | (ch >> 18) );
        writer.write( 0x80 | ((ch >> 12) & 0x3F) );
        writer.write( 0x80 | ((ch >> 6) & 0x3F) );
        writer.write( 0x80 | (ch & 0x3F) );

        if ((ch === UTF8ByteSwapNotAChar) || (ch === UTF8NotAChar))
          hasError = true;
      } else if (ch <= 0x3FFFFFF) {
        writer.write( 0xF0 | (ch >> 24) );
        writer.write( 0x80 | ((ch >> 18) & 0x3F) );
        writer.write( 0x80 | ((ch >> 12) & 0x3F) );
        writer.write( 0x80 | ((ch >> 6) & 0x3F) );
        writer.write( 0x80 | (ch & 0x3F) );

        hasError = true;
      } else if (ch <= 0x7FFFFFFF) {
        writer.write( 0xF0 | (ch >> 30) );
        writer.write( 0x80 | ((ch >> 24) & 0x3F) );
        writer.write( 0x80 | ((ch >> 18) & 0x3F) );
        writer.write( 0x80 | ((ch >> 12) & 0x3F) );
        writer.write( 0x80 | ((ch >> 6) & 0x3F) );
        writer.write( 0x80 | (ch & 0x3F) );

        hasError = true;
      } else {
        hasError = true;
      }
    }

    return writer.getResult();
  });

  $.RawMethod(false, "$decode", function UTF8Encoding_Decode (bytes, index, count) {
    // http://tidy.sourceforge.net/cgi-bin/lxr/source/src/utf8.c

    var reader = this.$makeByteReader(bytes, index, count), firstByte;
    var result = "";

    while (!reader.eof) {
      var accumulator = 0, extraBytes = 0, hasError = false;
      firstByte = reader.read();

      if (firstByte === false)
        continue;

      if (firstByte <= 0x7F) {
        accumulator = firstByte;
      } else if ((firstByte & 0xE0) === 0xC0) {
        accumulator = firstByte & 31;
        extraBytes = 1;
      } else if ((firstByte & 0xF0) === 0xE0) {
        accumulator = firstByte & 15;
        extraBytes = 2;
      } else if ((firstByte & 0xF8) === 0xF0) {
        accumulator = firstByte & 7;
        extraBytes = 3;
      } else if ((firstByte & 0xFC) === 0xF8) {
        accumulator = firstByte & 3;
        extraBytes = 4;
        hasError = true;
      } else if ((firstByte & 0xFE) === 0xFC) {
        accumulator = firstByte & 3;
        extraBytes = 5;
        hasError = true;
      } else {
        accumulator = firstByte;
        hasError = false;
      }

      while (extraBytes > 0) {
        var extraByte = reader.read();        
        extraBytes--;        

        if (extraByte === false) {
          hasError = true;
          break;
        }

        if ((extraByte & 0xC0) !== 0x80) {
          hasError = true;
          break;
        }

        accumulator = (accumulator << 6) | (extraByte & 0x3F);
      }

      if ((accumulator === UTF8ByteSwapNotAChar) || (accumulator === UTF8NotAChar))
        hasError = true;

      var characters;
      if (!hasError)
        characters = this.$fromCharCode(accumulator);

      if (hasError || (characters === false)) {
        if (this.throwOnInvalid)
          throw new Error("Invalid character in UTF8 text");
        else
          result += this.fallbackCharacter;
      } else
        result += characters;
    }

    return result;
  });
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.UTF8Encoding", true, [], function ($) {
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.UTF7Encoding", true, [], function ($) {
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.UnicodeEncoding", true, [], function ($) {
});

JSIL.ImplementExternals(
  "System.TimeSpan", function ($) {
    $.Method({Static:true , Public:true }, "FromMilliseconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMilliseconds (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * 10000);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromMinutes", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMinutes (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * 60 * 10000000);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromSeconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromSeconds (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * 10000000);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromTicks", 
      (new JSIL.MethodSignature($.Type, [$.Int64], [])), 
      function FromTicks (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "op_Addition", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Addition (t1, t2) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = t1._ticks + t2._ticks;
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "op_Equality", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_Equality (t1, t2) {
        return t1._ticks === t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_GreaterThan", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_GreaterThan (t1, t2) {
        return t1._ticks > t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_Inequality", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_Inequality (t1, t2) {
        return t1._ticks !== t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_LessThan", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_LessThan (t1, t2) {
        return t1._ticks < t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_Subtraction", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Subtraction (t1, t2) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = t1._ticks - t2._ticks;
        return result;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.Int64], [])), 
      function _ctor (ticks) {
        this._ticks = ticks;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (hours, minutes, seconds) {
        this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * hours)));
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds) {
        this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds, milliseconds) {
        this._ticks = 10000 * (milliseconds + 1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
      }
    );

    $.Method({Static:false, Public:true }, "get_Days", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Days () {
        return Math.floor((this._ticks / 10000000) / (60 * 60 * 24));
      }
    );

    $.Method({Static:false, Public:true }, "get_Hours", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Hours () {
        return Math.floor((this._ticks / 10000000) / (60 * 60)) % 24;
      }
    );

    $.Method({Static:false, Public:true }, "get_Milliseconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Milliseconds () {
        return Math.floor(this._ticks / 10000) % 1000;
      }
    );

    $.Method({Static:false, Public:true }, "get_Minutes", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Minutes () {
        return Math.floor((this._ticks / 10000000) / 60) % 60;
      }
    );

    $.Method({Static:false, Public:true }, "get_Seconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Seconds () {
        return Math.floor(this._ticks / 10000000) % 60;
      }
    );

    $.Method({Static:false, Public:true }, "get_Ticks", 
      (new JSIL.MethodSignature($.Int64, [], [])), 
      function get_Ticks () {
        return this._ticks;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMilliseconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMilliseconds () {
        return this._ticks / 10000;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMinutes", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMinutes () {
        return this._ticks / 600000000;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalSeconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalSeconds () {
        return this._ticks / 10000000;
      }
    );
  }
);

JSIL.MakeStruct("System.ValueType", "System.TimeSpan", true, [], function ($) {
  $.Field({Static:false, Public:false}, "_ticks", $.Int64, function ($) {
    return 0;
  });

  $.Property({Public: true , Static: false}, "Ticks");

  $.Property({Public: true , Static: false}, "Milliseconds");

  $.Property({Public: true , Static: false}, "TotalMilliseconds");

  $.Property({Public: true , Static: false}, "Seconds");

  $.Property({Public: true , Static: false}, "Minutes");

  $.Property({Public: true , Static: false}, "Hours");

  $.Property({Public: true , Static: false}, "Days");

  $.Property({Public: true , Static: false}, "TotalSeconds");

  $.Property({Public: true , Static: false}, "TotalMinutes");
});

$jsilcore.hashContainerBase = function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.RawMethod(false, "$areEqual", function HashContainer_AreEqual (lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  });

  $.RawMethod(false, "$searchBucket", function HashContainer_SearchBucket (key) {
    var hashCode = JSIL.ObjectHashCode(key);
    var bucket = this._dict[hashCode];
    if (!JSIL.IsArray(bucket))
      return null;

    for (var i = 0; i < bucket.length; i++) {
      var bucketEntry = bucket[i];

      if (this.$areEqual(bucketEntry[0], key))
        return bucketEntry;
    }

    return null;
  });

  $.RawMethod(false, "$removeByKey", function HashContainer_Remove (key) {
    var hashCode = JSIL.ObjectHashCode(key);
    var bucket = this._dict[hashCode];
    if (!JSIL.IsArray(bucket))
      return false;

    for (var i = 0; i < bucket.length; i++) {
      var bucketEntry = bucket[i];

      if (this.$areEqual(bucketEntry[0], key)) {
        bucket.splice(i, 1);
        this._count -= 1;
        return true;
      }
    }

    return false;
  });

  $.RawMethod(false, "$addToBucket", function HashContainer_Add (key, value) {
    var hashCode = JSIL.ObjectHashCode(key);
    var bucket = this._dict[hashCode];
    if (!JSIL.IsArray(bucket))
      this._dict[hashCode] = bucket = [];

    bucket.push([key, value]);
    this._count += 1;
    return value;
  });
};

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2", $jsilcore.hashContainerBase);

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this._dict = {};
      this._count = 0;
      this.tKeysEnumerator = null;
      this.tValuesEnumerator = null;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (capacity) {
      this._dict = {};
      this._count = 0;
      this.tKeysEnumerator = null;
      this.tValuesEnumerator = null;
    }
  );

  $.Method({Static:false, Public:true }, "Add", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")], [])), 
    function Add (key, value) {
      var bucketEntry = this.$searchBucket(key);

      if (bucketEntry !== null)
        throw new System.ArgumentException("Key already exists");

      return this.$addToBucket(key, value);
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Clear () {
      this._dict = {}
      this._count = 0;
    }
  );

  $.Method({Static:false, Public:true }, "ContainsKey", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])), 
    function ContainsKey (key) {
      return this.$searchBucket(key) !== null;
    }
  );

  $.Method({Static:false, Public:true }, "Remove", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])), 
    function Remove (key) {
      return this.$removeByKey(key);
    }
  );

  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      return this._count;
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2"), [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])), 
    function get_Item (key) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null)
        return bucketEntry[1];
      else
        throw new System.Exception("Key not found");
    }
  );

  $.Method({Static:false, Public:true }, "get_Keys", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.Dictionary`2/KeyCollection", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])), 
    function get_Keys () {
      if (this.tKeysEnumerator === null) {
        this.tKeysEnumerator = JSIL.ArrayEnumerator.Of(this.TKey);
      }

      return new JSIL.AbstractEnumerable(
        (function getKeysProxy () {
          var keys = [];

          for (var k in this._dict) {
            if (!this._dict.hasOwnProperty(k))
              continue;
            var bucket = this._dict[k];

            for (var i = 0; i < bucket.length; i++)
              keys.push(bucket[i][0]);
          }

          return new (this.tKeysEnumerator)(keys, -1);
        }).bind(this)
      );
    }
  );

  $.Method({Static:false, Public:true }, "get_Values", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.Dictionary`2/ValueCollection", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])), 
    function get_Values () {
      if (this.tValuesEnumerator === null) {
        this.tValuesEnumerator = JSIL.ArrayEnumerator.Of(this.TValue);
      }

      return new JSIL.AbstractEnumerable(
        (function getValuesProxy () {
          var values = [];

          for (var k in this._dict) {
            if (!this._dict.hasOwnProperty(k))
              continue;
            var bucket = this._dict[k];

            for (var i = 0; i < bucket.length; i++)
              values.push(bucket[i][1]);
          }

          return new (this.tValuesEnumerator)(values, -1);
        }).bind(this)
      );
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.Dictionary`2/Enumerator", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])), 
    function GetEnumerator () {
      var dict = this._dict;
      var tKvp = System.Collections.Generic.KeyValuePair$b2.Of(this.TKey, this.TValue);
      var tKey = this.TKey, tValue = this.TValue;

      return new JSIL.AbstractEnumerator(
        function getNext (result) {
          var keys = this._state.keys;
          var valueIndex = ++(this._state.valueIndex);
          var bucketIndex = this._state.bucketIndex;

          while ((bucketIndex >= 0) && (bucketIndex < keys.length)) {
            var bucketKey = keys[this._state.bucketIndex];
            var bucket = dict[bucketKey];

            if ((valueIndex >= 0) && (valueIndex < bucket.length)) {
              var current = this._state.current;
              current.key = bucket[valueIndex][0];
              current.value = bucket[valueIndex][1];
              result.value = current;
              return true;
            } else {
              bucketIndex = ++(this._state.bucketIndex);
              valueIndex = 0;
            }
          }

          return false;
        },
        function reset () {
          this._state = {
            current: new tKvp(JSIL.DefaultValue(tKey), JSIL.DefaultValue(tValue)),
            keys: Object.keys(dict),
            bucketIndex: 0,
            valueIndex: -1
          };
        },
        function dispose () {
          this._state = null;
        }
      );
    }
  );

  $.Method({Static:false, Public:true }, "set_Item", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")], [])), 
    function set_Item (key, value) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null)
        return bucketEntry[1] = value;
      else
        return this.$addToBucket(key, value);
    }
  );

  $.Method({Static:false, Public:true }, "TryGetValue", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), $jsilcore.TypeRef("JSIL.Reference", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])], [])), 
    function TryGetValue (key, /* ref */ value) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null) {
        value.value = bucketEntry[1];
        return true;
      } else {
        value.value = JSIL.DefaultValue(this.TValue);
      }

      return false;
    }
  );

});

JSIL.ImplementExternals("System.Collections.Generic.KeyValuePair`2", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.KeyValuePair`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.KeyValuePair`2")], [])), 
    function _ctor (key, value) {
      this.key = key;
      this.value = value;
    }
  );

  $.Method({Static:false, Public:true }, "get_Key", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TKey", "System.Collections.Generic.KeyValuePair`2"), [], [])), 
    function get_Key () {
      return this.key;
    }
  );

  $.Method({Static:false, Public:true }, "get_Value", 
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.KeyValuePair`2"), [], [])), 
    function get_Value () {
      return this.value;
    }
  );

  $.Method({Static:false, Public:true }, "toString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function toString () {
      return "[" + String(this.key) + ", " + String(this.value) + "]";
    }
  );

});

JSIL.MakeStruct("System.ValueType", "System.Collections.Generic.KeyValuePair`2", true, ["TKey", "TValue"], function ($) {
  $.Field({Static:false, Public:false}, "key", $.GenericParameter("TKey"));

  $.Field({Static:false, Public:false}, "value", $.GenericParameter("TValue"));

  $.Property({Static:false, Public:true }, "Key");

  $.Property({Static:false, Public:true }, "Value");
});

JSIL.MakeClass("System.Object", "System.Collections.Generic.Dictionary`2", true, ["TKey", "TValue"], function ($) {
  $.Property({Public: true , Static: false}, "Count");
  $.Property({Public: true , Static: false}, "Keys");
  $.Property({Public: true , Static: false}, "Values");

  $.ImplementInterfaces(
//      $jsilcore.TypeRef("System.Collections.Generic.IDictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), $asm07.TypeRef("System.Collections.Generic.ICollection`1", [$asm07.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]), 
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]), 
//      $jsilcore.TypeRef("System.Collections.IDictionary"), 
//      $jsilcore.TypeRef("System.Collections.ICollection"), $asm07.TypeRef("System.Collections.IEnumerable"), 
      $jsilcore.TypeRef("System.Collections.IEnumerable")
  );
});

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.Collections.Generic.Dictionary`2/Enumerator", false, ["TKey", "TValue"], function ($) {

  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2/Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2/Enumerator")])]), 
      $jsilcore.TypeRef("System.IDisposable"), 
//      $jsilcore.TypeRef("System.Collections.IDictionaryEnumerator"), 
      $jsilcore.TypeRef("System.Collections.IEnumerator")
  )
});

$jsilcore.$tArrayEnumerator = null;

JSIL.MakeArrayEnumerator = function (array) {
  if ($jsilcore.$tArrayEnumerator === null)
    $jsilcore.$tArrayEnumerator = JSIL.ArrayEnumerator.Of(System.Object);

  return new ($jsilcore.$tArrayEnumerator) (array, -1);
};

JSIL.GetEnumerator = function (enumerable) {
  if ((typeof (enumerable) === "undefined") || (enumerable === null))
    throw new Error("Enumerable is null or undefined");

  if (JSIL.IsArray(enumerable))
    return JSIL.MakeArrayEnumerator(enumerable);
  else if (typeof (enumerable.IEnumerable$b1_GetEnumerator) === "function")
    return enumerable.IEnumerable$b1_GetEnumerator();
  else if (typeof (enumerable.IEnumerable_GetEnumerator) === "function")
    return enumerable.IEnumerable_GetEnumerator();    
  else if (typeof (enumerable.GetEnumerator) === "function")
    return enumerable.GetEnumerator();    
  else if (typeof (enumerable) === "string")
    return JSIL.MakeArrayEnumerator(enumerable);
  else
    throw new Error("Value is not enumerable");
};

JSIL.EnumerableToArray = function (enumerable) {
  var e = JSIL.GetEnumerator(enumerable);
  var result = [];

  try {
    while (e.IEnumerator_MoveNext())
      result.push(e.IEnumerator_Current);
  } finally {
    e.IDisposable_Dispose();
  }

  return result;
};

JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerator", true, [], function ($) {
  $.RawMethod(false, "__CopyMembers__", 
    function AbstractEnumerator_CopyMembers (source, target) {
      target._getNextItem = source._getNextItem;
      target._reset = source._reset;
      target._dispose = source._dispose;
      target._first = source._first;
      target._needDispose = source._needDispose;
      target._current = new JSIL.Variable(source._current.value);
      target._state = source._state;
    }
  );

  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [JSIL.AnyType, JSIL.AnyType, JSIL.AnyType]),
    function (getNextItem, reset, dispose) {
      this._getNextItem = getNextItem;
      this._reset = reset;
      this._dispose = dispose;
      this._first = true;
      this._needDispose = false;
      this._current = new JSIL.Variable(null);
    }
  );

  $.Method({Static: false, Public: true }, "Reset",
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._needDispose)
        this._dispose();

      this._first = false;
      this._needDispose = true;
      this._reset();
    }
  );

  $.Method({Static: false, Public: true }, "MoveNext",
    new JSIL.MethodSignature("System.Boolean", []),
    function () {
      if (this._first) {
        this._reset();
        this._needDispose = true;
        this._first = false;
      }

      return this._getNextItem(this._current);
    }
  );

  $.Method({Static: false, Public: true }, "Dispose",
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._needDispose)
        this._dispose();

      this._needDispose = false;
    }
  );


  $.Method({Static: false, Public: true }, "get_Current",
    new JSIL.MethodSignature(JSIL.AnyType, []),
    function () {
      return this._current.value;
    }
  );

  $.Property({Static: false, Public: true, Virtual: true }, "Current");

  $.ImplementInterfaces(
    System.IDisposable, System.Collections.IEnumerator, System.Collections.Generic.IEnumerator$b1
  );
});

JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerable", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [JSIL.AnyType, JSIL.AnyType, JSIL.AnyType]),
    function (getNextItem, reset, dispose) {
      if (arguments.length === 1) {
        this._getEnumerator = getNextItem;
      } else {
        this._getEnumerator = null;
        this._getNextItem = getNextItem;
        this._reset = reset;
        this._dispose = dispose;
      }
    }
  );

  $.Method({Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature("System.Collections.IEnumerator", []),
    function () {
      if (this._getEnumerator !== null)
        return this._getEnumerator();
      else
        return new JSIL.AbstractEnumerator(this._getNextItem, this._reset, this._dispose);
    }
  );

  $.ImplementInterfaces(
    System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
  );
});

JSIL.ImplementExternals(
  "System.Linq.Enumerable", function ($) {
    $.Method({Static:true , Public:true }, "Any", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable);

        try {
          if (enumerator.IEnumerator_MoveNext())
            return true;
        } finally {
          enumerator.IDisposable_Dispose();
        }

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "Any", 
      new JSIL.MethodSignature(
        $jsilcore.TypeRef("System.Boolean"), 
        [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", $jsilcore.TypeRef("System.Boolean")])], 
        ["TSource"]
      ),
      function (T, enumerable, predicate) {
        var enumerator = JSIL.GetEnumerator(enumerable);
        
        try {
          while (enumerator.IEnumerator_MoveNext()) {
            if (predicate(enumerator.IEnumerator_Current))
              return true;
          }
        } finally {
          enumerator.IDisposable_Dispose();
        }

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "Count", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var e = JSIL.GetEnumerator(enumerable);
        var result = 0;
        try {
          while (e.IEnumerator_MoveNext())
            result += 1;
        } finally {
          e.IDisposable_Dispose();
        }
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "First", 
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable);
        try {
          if (enumerator.IEnumerator_MoveNext())
            return enumerator.IEnumerator_Current;
        } finally {
          enumerator.IDisposable_Dispose();
        }

        throw new System.Exception("Enumerable contains no items");
      }
    );

    $.Method({Static:true , Public:true }, "Select", 
      new JSIL.MethodSignature(
        $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!1"]), 
        [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", "!!1"])], 
        ["TSource", "TResult"]
      ),
      function (TSource, TResult, enumerable, selector) {
        var state = {};

        return new JSIL.AbstractEnumerable(
          function getNext (result) {
            var ok = state.enumerator.IEnumerator_MoveNext();
            if (ok)
              result.value = selector(state.enumerator.IEnumerator_Current);

            return ok;
          },
          function reset () {
            state.enumerator = JSIL.GetEnumerator(enumerable);
          },
          function dispose () {
            state.enumerator.IDisposable_Dispose();
          }
        );
      }
    );
    
    $.Method({Static:true , Public:true }, "ToArray", 
      new JSIL.MethodSignature(System.Array.Of("!!0"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        return JSIL.EnumerableToArray(enumerable);
      }
    );

    $.Method({Static:true , Public:true }, "Contains", 
      (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), "!!0"], ["TSource"])), 
      function Contains$b1 (TSource, source, item) {
        var enumerator = JSIL.GetEnumerator(source);

        try {
          while (enumerator.IEnumerator_MoveNext()) {
            if (JSIL.ObjectEquals(enumerator.IEnumerator_Current, item))
              return true;
          }
        } finally {
          enumerator.IDisposable_Dispose();
        }

        return false;
      }
    );    

  }
);

JSIL.MakeStaticClass("System.Linq.Enumerable", true, [], function ($) {
  $.ExternalMethod({Static:true , Public:true }, "Any", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Any", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", $jsilcore.TypeRef("System.Boolean")])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Count", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "First", 
    new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Select", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!1"]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", "!!1"])], ["TSource", "TResult"])
  );

  $.ExternalMethod({Static:true , Public:true }, "ToArray", 
    new JSIL.MethodSignature(System.Array.Of("!!0"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );
});

JSIL.ImplementExternals("System.Nullable", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:true , Public:true }, "GetUnderlyingType", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Type"), [mscorlib.TypeRef("System.Type")], [])), 
    function GetUnderlyingType (nullableType) {
      if (nullableType.__FullName__.indexOf("System.Nullable`1") !== 0) {
        return null;
      } else {
        return nullableType.__PublicInterface__.T;
      }
    }
  );
});

JSIL.MakeStaticClass("System.Nullable", true, [], function ($) {
});

JSIL.ImplementExternals("System.Nullable`1", function ($) {
  $.RawMethod(true, "CheckType", function (value) {
    if (this.T.$Is(value))
      return true;

    return false;    
  });
});

JSIL.MakeStruct("System.ValueType", "System.Nullable`1", true, ["T"], function ($) {
});

JSIL.MakeEnum("System.Reflection.BindingFlags", true, $jsilcore.BindingFlags, true);

JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializer", function ($) {
});

JSIL.MakeEnum(
  "System.StringComparison", true, {
    CurrentCulture: 0, 
    CurrentCultureIgnoreCase: 1, 
    InvariantCulture: 2, 
    InvariantCultureIgnoreCase: 3, 
    Ordinal: 4, 
    OrdinalIgnoreCase: 5
  }, false
);

JSIL.ImplementExternals("System.Text.StringBuilder", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this._str = "";
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (capacity) {
      this._str = "";
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function _ctor (value) {
      this._str = value;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String, $.Int32], [])), 
    function _ctor (value, capacity) {
      this._str = value;
    }
  );

  var appendString = function (self, str, startIndex, length, copies) {
    if (arguments.length === 2) {
      startIndex = 0;
      length = str.length;
      copies = 1;
    }

    if ((startIndex !== 0) || (length !== str.length)) {
      for (var i = 0; i < copies; i++) {
        self._str += str.substr(startIndex, length);
      }

    } else {
      for (var i = 0; i < copies; i++) {
        self._str += str;
      }

    }
  };

  var appendNumber = function (self, num) {
    self._str += String(num);
  };

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Char, $.Int32], [])), 
    function Append (value, repeatCount) {
      appendString(this, value, 0, value.length, repeatCount);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32, 
          $.Int32
        ], [])), 
    function Append (value, startIndex, charCount) {
      for (var i = 0; i < charCount; i++)
        this._str += value[startIndex + i];
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.String], [])), 
    function Append (value) {
      appendString(this, value, 0, value.length, 1);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Int32, 
          $.Int32
        ], [])), 
    function Append (value, startIndex, count) {
      appendString(this, value, startIndex, count, 1);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Boolean], [])), 
    function Append (value) {
      this._str += (value ? "True" : "False");
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.SByte], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Byte], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Char], [])), 
    function Append (value) {
      appendString(this, value, 0, value.length, 1);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Int16], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Int32], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Int64], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Single], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.Double], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.UInt16], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.UInt32], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$.UInt64], [])), 
    function Append (value) {
      appendNumber(this, value);
    }
  );

  $.Method({Static:false, Public:true }, "Append", 
    (new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Array", [$.Char])], [])), 
    function Append (value) {
      for (var i = 0; i < value.length; i++)
        this._str += value[i];
    }
  );

  $.Method({Static:false, Public:true }, "AppendLine", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.StringBuilder"), [], [])), 
    function AppendLine () {
      appendString(this, "\r\n", 0, 2, 1);
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "AppendLine", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.StringBuilder"), [$.String], [])), 
    function AppendLine (value) {
      appendString(this, value, 0, value.length, 1);
      appendString(this, "\r\n", 0, 2, 1);
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "AppendFormat", 
    (new JSIL.MethodSignature($.Type, [$.String, $.Object], [])), 
    function AppendFormat (format, arg0) {
      appendString(this, System.String.Format(format, [arg0]));
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "AppendFormat", 
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Object, 
          $.Object
        ], [])), 
    function AppendFormat (format, arg0, arg1) {
      appendString(this, System.String.Format(format, [arg0, arg1]));
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "AppendFormat", 
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Object, 
          $.Object, $.Object
        ], [])), 
    function AppendFormat (format, arg0, arg1, arg2) {
      appendString(this, System.String.Format(format, [arg0, arg1, arg2]));
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "AppendFormat", 
    (new JSIL.MethodSignature($.Type, [$.String, $jsilcore.TypeRef("System.Array", [$.Object])], [])), 
    function AppendFormat (format, args) {
      appendString(this, System.String.Format(format, args));
      return this;
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    (new JSIL.MethodSignature($.Type, [], [])), 
    function Clear () {
      this._str = "";
    }
  );

  $.Method({Static:false, Public:true }, "get_Length", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Length () {
      return this._str.length;
    }
  );

  var replace = function (self, oldText, newText, startIndex, count) {
    var prefix = self._str.substr(0, startIndex);
    var suffix = self._str.substr(startIndex + count);
    var region = self._str.substr(startIndex, count);
    var result = prefix + region.split(oldText).join(newText) + suffix;
    self._str = result;
    return self;
  };

  $.Method({Static:false, Public:true }, "Replace", 
    (new JSIL.MethodSignature($.Type, [$.String, $.String], [])), 
    function Replace (oldValue, newValue) {
      return replace(this, oldValue, newValue, 0, this._str.length);
    }
  );

  $.Method({Static:false, Public:true }, "Replace", 
    (new JSIL.MethodSignature($.Type, [
          $.String, $.String, 
          $.Int32, $.Int32
        ], [])), 
    function Replace (oldValue, newValue, startIndex, count) {
      return replace(this, oldValue, newValue, startIndex, count);
    }
  );

  $.Method({Static:false, Public:true }, "Replace", 
    (new JSIL.MethodSignature($.Type, [$.Char, $.Char], [])), 
    function Replace (oldChar, newChar) {
      return replace(this, oldChar, newChar, 0, this._str.length);
    }
  );

  $.Method({Static:false, Public:true }, "Replace", 
    (new JSIL.MethodSignature($.Type, [
          $.Char, $.Char, 
          $.Int32, $.Int32
        ], [])), 
    function Replace (oldChar, newChar, startIndex, count) {
      return replace(this, oldChar, newChar, startIndex, count);
    }
  );

  $.Method({Static:false, Public:true }, "set_Length", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Length (value) {
      var delta = value - this._str.length;

      if (delta < 0) {
        this._str = this._str.substr(0, value);
      } else if (delta > 0) {
        var ch = new Array(delta);
        for (var i = 0; i < delta; i++)
          ch[i] = '\0';

        this._str += JSIL.StringFromByteArray(ch);
      }
    }
  );

  $.Method({Static:false, Public:true }, "toString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function toString () {
      return this._str;
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Text.StringBuilder", true, [], function ($) {
});

JSIL.ImplementExternals("System.Diagnostics.StackTrace", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.CaptureStackTrace(0, false, null, null);
    }
  );

  $.Method({Static:false, Public:false}, "CaptureStackTrace", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Boolean, 
          mscorlib.TypeRef("System.Threading.Thread"), mscorlib.TypeRef("System.Exception")
        ], [])), 
    function CaptureStackTrace (iSkip, fNeedFileInfo, targetThread, e) {
      // FIXME
      this.frames = [];
    }
  );  

  $.Method({Static:false, Public:true }, "GetFrame", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Diagnostics.StackFrame"), [$.Int32], [])), 
    function GetFrame (index) {
      // FIXME
      return new System.Diagnostics.StackFrame();
    }
  );

});

JSIL.ImplementExternals("System.Diagnostics.StackFrame", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "GetMethod", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Reflection.MethodBase"), [], [])), 
    function GetMethod () {
      // FIXME
      return new System.Reflection.MethodBase();
    }
  );
});

JSIL.ImplementExternals(
  "System.Enum", function ($) {    
    $.RawMethod(true, "CheckType",
      function (value) {
        if (typeof (value) === "object") {
          if ((value !== null) && (typeof (value.GetType) === "function"))
            return value.GetType().IsEnum;
        }

        return false;
      }
    );

    var internalTryParse;

    var internalTryParseFlags = function (TEnum, text, ignoreCase, result) {
      var items = text.split(",");

      var resultValue = 0;
      var temp = new JSIL.Variable();

      for (var i = 0, l = items.length; i < l; i++) {
        var item = items[i].trim();
        if (item.length === 0)
          continue;

        if (internalTryParse(TEnum, item, ignoreCase, temp)) {
          resultValue = resultValue | temp.value;
        } else {
          return false;
        }
      }

      var name = TEnum.__ValueToName__[resultValue];

      if (typeof (name) === "undefined") {
        result.value = TEnum.$MakeValue(resultValue, null);
        return true;
      } else {
        result.value = TEnum[name];
        return true;
      }
    };

    internalTryParse = function (TEnum, text, ignoreCase, result) {
      // Detect and handle flags enums
      var commaPos = text.indexOf(",");
      if (commaPos >= 0)
        return internalTryParseFlags(TEnum, text, ignoreCase, result);

      var num = parseInt(text, 10);

      if (isNaN(num)) {
        if (ignoreCase) {
          var names = TEnum.__Names__;
          for (var i = 0; i < names.length; i++) {
            var isMatch = (names[i].toLowerCase() == text.toLowerCase());

            if (isMatch) {
              result.value = TEnum[names[i]];
              break;
            }
          }
        } else {
          result.value = TEnum[text];
        }

        return (typeof (result.value) !== "undefined");
      } else {
        var name = TEnum.__ValueToName__[num];

        if (typeof (name) === "undefined") {
          result.value = TEnum.$MakeValue(num, null);
          return true;
        } else {
          result.value = TEnum[name];
          return true;
        }
      }
    };

    var internalParse = function (enm, text, ignoreCase) {
      var result = new JSIL.Variable();
      if (internalTryParse(enm, text, ignoreCase, result))
        return result.value;

      throw new System.Exception("Failed to parse enum");
    };

    $.Method({Static:true , Public:true }, "Parse", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [$jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.String")], []),
      function (enm, text) {
        return internalParse(enm, text, false);
      }
    );

    $.Method({Static:true , Public:true }, "Parse", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [
          $jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.String"), 
          $jsilcore.TypeRef("System.Boolean")
        ], []),
      internalParse
    );    

    $.Method({Static:true , Public:true }, "TryParse", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String"), "JSIL.Reference" /* !!0& */ ], ["TEnum"]),
      function (TEnum, text, result) {
        return internalTryParse(TEnum, text, result);
      }
    );

    $.Method({Static:true , Public:true }, "TryParse", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.Boolean"), 
          "JSIL.Reference" /* !!0& */ 
        ], ["TEnum"]),
      internalTryParse
    );

    $.Method({Static:true , Public:true }, "GetNames", 
      new JSIL.MethodSignature(System.Array.Of($jsilcore.System.String), [$jsilcore.TypeRef("System.Type")], []),
      function (enm) {
        return enm.__Names__;
      }
    );

    $.Method({Static:true , Public:true }, "GetValues", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array"), [$jsilcore.TypeRef("System.Type")], []),
      function (enm) {
        var names = enm.__Names__;
        var result = new Array(names.length);

        for (var i = 0; i < result.length; i++)
          result[i] = enm[names[i]];

        return result;
      }
    );
  }
);

JSIL.ImplementExternals("System.Activator", function ($) {
  var mscorlib = JSIL.GetCorlib();
  
  $.Method({Static:true , Public:true }, "CreateInstance", 
    (new JSIL.MethodSignature($.Object, [mscorlib.TypeRef("System.Type"), mscorlib.TypeRef("System.Array", [$.Object])], [])), 
    function CreateInstance (type, args) {
      if (!args)
        args = [];

      return JSIL.CreateInstanceOfType(type, args);
    }
  );

  $.Method({Static:true , Public:true }, "CreateInstance", 
    (new JSIL.MethodSignature($.Object, [
          $jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.Reflection.BindingFlags"), 
          $jsilcore.TypeRef("System.Reflection.Binder"), $jsilcore.TypeRef("System.Array", [$.Object]), 
          $jsilcore.TypeRef("System.Globalization.CultureInfo")
        ], [])), 
    function CreateInstance (type, bindingAttr, binder, args, culture) {
      // FIXME
      if (!args)
        args = [];
      
      return JSIL.CreateInstanceOfType(type, args);
    }
  );

});

JSIL.ImplementExternals("System.Diagnostics.Stopwatch", function ($) {
  var mscorlib = JSIL.GetCorlib(); 

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.Reset();
    }
  );

  $.Method({Static:false, Public:true }, "get_Elapsed", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.TimeSpan"), [], [])), 
    function get_Elapsed () {
      return System.TimeSpan.FromMilliseconds(this.get_ElapsedMilliseconds());
    }
  );

  $.Method({Static:false, Public:true }, "get_ElapsedMilliseconds", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_ElapsedMilliseconds () {
      var result = this.elapsed;
      if (this.isRunning)
        result += Date.now() - this.startedWhen;

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "get_ElapsedTicks", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_ElapsedTicks () {
      return this.get_ElapsedMilliseconds() * 10000;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsRunning", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsRunning () {
      return this.isRunning;
    }
  );

  $.Method({Static:false, Public:true }, "Reset", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Reset () {
      this.elapsed = 0;
      this.isRunning = false;
      this.startedWhen = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Restart", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Restart () {
      this.elapsed = 0;
      this.isRunning = true;
      this.startedWhen = Date.now();
    }
  );

  $.Method({Static:false, Public:true }, "Start", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Start () {
      if (!this.isRunning) {
        this.startedWhen = Date.now();
        this.isRunning = true;
      }
    }
  );

  $.Method({Static:true , Public:true }, "StartNew", 
    (new JSIL.MethodSignature($.Type, [], [])), 
    function StartNew () {
      var result = new System.Diagnostics.Stopwatch();
      result.Start();
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "Stop", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Stop () {
      if (this.isRunning) {
        this.isRunning = false;

        var now = Date.now();
        var elapsed = now - this.startedWhen;

        this.elapsed += elapsed;
        if (this.elapsed < 0)
          this.elapsed = 0;
      }
    }
  );

});

JSIL.ImplementExternals("System.Text.RegularExpressions.Regex", function ($) {
  var system = JSIL.GetAssembly("System", true);

  var makeRegex = function (pattern, options) {
    if ((options & System.Text.RegularExpressions.RegexOptions.ECMAScript) === 0) {
      throw new Error("Non-ECMAScript regexes are not currently supported.");
    }

    var flags = "g";

    if ((options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) !== 0) {
      flags += "i";
    }

    if ((options & System.Text.RegularExpressions.RegexOptions.Multiline) !== 0) {
      flags += "m";
    }

    return new RegExp(pattern, flags);
  };

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function _ctor (pattern) {
      this._regex = makeRegex(pattern, System.Text.RegularExpressions.RegexOptions.None);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String, system.TypeRef("System.Text.RegularExpressions.RegexOptions")], [])), 
    function _ctor (pattern, options) {
      this._regex = makeRegex(pattern, options);
    }
  );

  $.Method({Static:false, Public:true }, "Matches", 
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.MatchCollection"), [$.String], [])), 
    function Matches (input) {
      var matches = input.match(this._regex);

      var matchObjects = [];
      var tMatch = System.Text.RegularExpressions.Match.__Type__;

      if (JSIL.IsArray(matches)) {
        for (var i = 0; i < matches.length; i++) {
          matchObjects.push(
            JSIL.CreateInstanceOfType(
              tMatch, "$internalCtor", [matches[i]]
            )
          );
        }
      }

      var result = JSIL.CreateInstanceOfType(
        System.Text.RegularExpressions.MatchCollection.__Type__,
        "$internalCtor", [matchObjects]
      );

      return result;
    }
  );

  $.Method({Static:true , Public:true }, "Replace", 
    (new JSIL.MethodSignature($.String, [
          $.String, $.String, 
          $.String, system.TypeRef("System.Text.RegularExpressions.RegexOptions")
        ], [])), 
    function Replace (input, pattern, replacement, options) {
      var re = makeRegex(pattern, options);

      return input.replace(re, replacement);
    }
  );
});

JSIL.ImplementExternals("System.Text.RegularExpressions.MatchCollection", function ($) {
  var system = JSIL.GetAssembly("System", true);
  var mscorlib = JSIL.GetCorlib();
  var tEnumerator = JSIL.ArrayEnumerator.Of(System.Text.RegularExpressions.Match);

  $.RawMethod(false, "$internalCtor", function (matches) {
    this._matches = matches;
  });

  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      return this._matches.length;
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.Match"), [$.Int32], [])), 
    function get_Item (i) {
      return this._matches[i];
    }
  );

  $.Method({Static:false, Public:false}, "GetMatch", 
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.Match"), [$.Int32], [])), 
    function GetMatch (i) {
      return this._matches[i];
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], [])), 
    function GetEnumerator () {
      return new tEnumerator(this._matches);
    }
  );
});

JSIL.ImplementExternals("System.Text.RegularExpressions.Capture", function ($) {

  $.Method({Static:false, Public:true }, "get_Length", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Length () {
      return this._length;
    }
  );

  $.Method({Static:false, Public:true }, "get_Value", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Value () {
      return this._text;
    }
  );

  $.Method({Static:false, Public:true }, "toString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function toString () {
      return this._text;
    }
  );
});

JSIL.ImplementExternals("System.Text.RegularExpressions.Match", function ($) {
  $.RawMethod(false, "$internalCtor", function (text) {
    this._text = text;
    this._length = text.length;
  });
});

JSIL.MakeStruct("System.ValueType", "System.EventArgs", true, [], function ($) {
  $.Field({Static:true , Public:true }, "Empty", $jsilcore.TypeRef("System.EventArgs"), function ($) {
    return new System.EventArgs();
  });
});

JSIL.ImplementExternals("System.Diagnostics.Debug", function ($) {

  $.Method({Static:true , Public:true }, "Assert", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Assert (condition) {
      if (!condition)
        JSIL.Host.assertionFailed("Assertion Failed");
    }
  );

  $.Method({Static:true , Public:true }, "Assert", 
    (new JSIL.MethodSignature(null, [$.Boolean, $.String], [])), 
    function Assert (condition, message) {
      if (!condition)
        JSIL.Host.assertionFailed(message);
    }
  );

});

JSIL.MakeEnum(
  "System.IO.FileMode", true, {
    CreateNew: 1, 
    Create: 2, 
    Open: 3, 
    OpenOrCreate: 4, 
    Truncate: 5, 
    Append: 6
  }, false
);

JSIL.ImplementExternals("System.GC", function ($) {
  var warnedAboutMemory = false;

  var warnIfNecessary = function () {
    if (warnedAboutMemory)
      return;

    warnedAboutMemory = true;

    JSIL.Host.warning("WARNING: JS heap memory statistics not available in your browser.");
  };

  var getMemoryImpl = function () {
    if (window && window.performance && window.performance.memory) {
      return window.performance.memory.usedJSHeapSize;
    } else {
      warnIfNecessary();
      return 0;
    }
  };

  $.Method({Static:true , Public:false}, "GetTotalMemory", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function GetTotalMemory () {
      return getMemoryImpl();
    }
  );

  $.Method({Static:true , Public:true }, "GetTotalMemory", 
    (new JSIL.MethodSignature($.Int64, [$.Boolean], [])), 
    function GetTotalMemory (forceFullCollection) {
      // FIXME: forceFullCollection

      return getMemoryImpl();
    }
  );

  $.Method({Static:true , Public:false}, "IsServerGC", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function IsServerGC () {
      return false;
    }
  );
});

JSIL.ImplementExternals("System.Collections.Generic.HashSet`1", $jsilcore.hashContainerBase);

JSIL.ImplementExternals("System.Collections.Generic.HashSet`1", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this._dict = {};
      this._count = 0;
      this._comparer = null;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function _ctor (comparer) {
      this._dict = {};
      this._count = 0;
      this._comparer = comparer;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function _ctor (collection) {
      this._dict = {};
      this._count = 0;
      this._comparer = null;
      this.$addRange(collection);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function _ctor (collection, comparer) {
      this._dict = {};
      this._count = 0;
      this._comparer = comparer;
      this.$addRange(collection);
    }
  );

  $.Method({Static:false, Public:true }, "Add", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function Add (item) {
      var bucketEntry = this.$searchBucket(item);

      if (bucketEntry !== null)
        return false;

      this.$addToBucket(item, true);
      return true;
    }
  );

  $.RawMethod(false, "$addRange", function (enumerable) {
    var values = JSIL.EnumerableToArray(enumerable);

    for (var i = 0; i < values.length; i++)
      this.Add(values[i]);
  });

  $.Method({Static:false, Public:false}, "AddOrGetLocation", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1"), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], [])), 
    function AddOrGetLocation (value, /* ref */ location) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "AreEqualityComparersEqual", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function AreEqualityComparersEqual (set1, set2) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "CheckUniqueAndUnfoundElements", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.HashSet`1/ElementCount", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $.Boolean], [])), 
    function CheckUniqueAndUnfoundElements (other, returnIfUnfound) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "Clear", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Clear () {
      this._dict = {};
      this._count = 0;
    }
  );

  $.Method({Static:false, Public:true }, "Contains", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function Contains (item) {
      return this.$searchBucket(item) !== null;
    }
  );

  $.Method({Static:false, Public:false}, "ContainsAllElements", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function ContainsAllElements (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "CopyTo", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $.Int32], [])), 
    function CopyTo (array, arrayIndex) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "CopyTo", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function CopyTo (array) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "CopyTo", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $.Int32, 
          $.Int32
        ], [])), 
    function CopyTo (array, arrayIndex, count) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "CreateSetComparer", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])]), [], [])), 
    function CreateSetComparer () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "ExceptWith", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function ExceptWith (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Comparer", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), [], [])), 
    function get_Comparer () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      return this._count;
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.HashSet`1/Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), [], [])), 
    function GetEnumerator () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetObjectData", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Runtime.Serialization.SerializationInfo"), $jsilcore.TypeRef("System.Runtime.Serialization.StreamingContext")], [])), 
    function GetObjectData (info, context) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "HashSetEquals", 
    (new JSIL.MethodSignature($.Boolean, [
          $jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), $jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), 
          $jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])
        ], [])), 
    function HashSetEquals (set1, set2, comparer) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IncreaseCapacity", 
    (new JSIL.MethodSignature(null, [], [])), 
    function IncreaseCapacity () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function Initialize (capacity) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "InternalGetHashCode", 
    (new JSIL.MethodSignature($.Int32, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function InternalGetHashCode (item) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "InternalIndexOf", 
    (new JSIL.MethodSignature($.Int32, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function InternalIndexOf (item) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "IntersectWith", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IntersectWith (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IntersectWithEnumerable", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IntersectWithEnumerable (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IntersectWithHashSetWithSameEC", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IntersectWithHashSetWithSameEC (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "IsProperSubsetOf", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IsProperSubsetOf (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "IsProperSupersetOf", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IsProperSupersetOf (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "IsSubsetOf", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IsSubsetOf (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IsSubsetOfHashSetWithSameEC", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IsSubsetOfHashSetWithSameEC (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "IsSupersetOf", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function IsSupersetOf (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "OnDeserialization", 
    (new JSIL.MethodSignature(null, [$.Object], [])), 
    function OnDeserialization (sender) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "Overlaps", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function Overlaps (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "Remove", 
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function Remove (item) {
      return this.$removeByKey(item);
    }
  );

  $.Method({Static:false, Public:true }, "RemoveWhere", 
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Predicate`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function RemoveWhere (match) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "SetEquals", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function SetEquals (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "SymmetricExceptWith", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function SymmetricExceptWith (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "SymmetricExceptWithEnumerable", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function SymmetricExceptWithEnumerable (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "SymmetricExceptWithUniqueHashSet", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function SymmetricExceptWithUniqueHashSet (other) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "ICollection`1.Add", 
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")], [])), 
    function ICollection$b1_Add (item) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "ICollection`1.get_IsReadOnly", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function ICollection$b1_get_IsReadOnly () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IEnumerable`1.GetEnumerator", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), [], [])), 
    function IEnumerable$b1_GetEnumerator () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "IEnumerable.GetEnumerator", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], [])), 
    function IEnumerable_GetEnumerator () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "ToArray", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), [], [])), 
    function ToArray () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "TrimExcess", 
    (new JSIL.MethodSignature(null, [], [])), 
    function TrimExcess () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "UnionWith", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])), 
    function UnionWith (other) {
      throw new Error('Not implemented');
    }
  );

});

JSIL.MakeClass("System.Object", "System.Collections.Generic.HashSet`1", true, ["T"], function ($) {
  $.Property({Public: true , Static: false}, "Count");

  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), 
      $jsilcore.TypeRef("System.Collections.IEnumerable")
//      $jsilcore.TypeRef("System.Collections.Generic.ISet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), 
//      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), 
  );
});

JSIL.MakeEnum(
  "System.Globalization.NumberStyles", true, {
    None: 0, 
    AllowLeadingWhite: 1, 
    AllowTrailingWhite: 2, 
    AllowLeadingSign: 4, 
    Integer: 7, 
    AllowTrailingSign: 8, 
    AllowParentheses: 16, 
    AllowDecimalPoint: 32, 
    AllowThousands: 64, 
    Number: 111, 
    AllowExponent: 128, 
    Float: 167, 
    AllowCurrencySymbol: 256, 
    Currency: 383, 
    Any: 511, 
    AllowHexSpecifier: 512, 
    HexNumber: 515
  }, true
);

JSIL.ImplementExternals("System.Convert", function ($) {
  $.Method({Static:true , Public:true }, "ChangeType", 
    (new JSIL.MethodSignature($.Object, [$.Object, $jsilcore.TypeRef("System.Type")], [])), 
    function ChangeType (value, conversionType) {
      // FIXME: Actually compatible?
      if (value && value.IConvertible_ToType) {
        // FIXME: provider
        return value.IConvertible_ToType(conversionType, null);
      } else {
        return conversionType.__PublicInterface__.$As(value);
      }
    }
  );

  var returnSame = function (value) {
    return value;
  };

  var makeAdapter = function (adapter) {
    if (!adapter)
      throw new Error("No adapter provided");

    return function (value) {
      return adapter(value);
    };
  };

  var boolToInt = function (b) {
    return b ? 1 : 0;
  };

  var boolToString = function (b) {
    return b ? "True" : "False";
  };

  var makeConvertMethods = function (typeName, to, from) {
    // FIXME: We currently ignore the format provider argument
    // FIXME: Range checks/clipping/saturation are not performed for the integer types

    var methodName = "To" + typeName;

    var descriptor = {Static:true , Public: true };
    var tFormatProvider = $jsilcore.TypeRef("System.IFormatProvider");

    var makeSignature = function (argType, formatProvider) {
      if (formatProvider)
        return new JSIL.MethodSignature(to, [argType, tFormatProvider], []);
      else
        return new JSIL.MethodSignature(to, [argType], []);
    };

    $.Method(descriptor, methodName, makeSignature($.Boolean), from.boolean);

    $.Method(descriptor, methodName, makeSignature($.Boolean, true), from.boolean);

    $.Method(descriptor, methodName, makeSignature($.SByte), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int16), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int32), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int64), from.int);

    $.Method(descriptor, methodName, makeSignature($.SByte, true), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int16, true), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int32, true), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int64, true), from.int);
    
    $.Method(descriptor, methodName, makeSignature($.Byte), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt16), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt32), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt64), from.uint);
    
    $.Method(descriptor, methodName, makeSignature($.Byte, true), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt16, true), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt32, true), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt64, true), from.uint);    

    $.Method(descriptor, methodName, makeSignature($.Single), from.float);
    $.Method(descriptor, methodName, makeSignature($.Double), from.float);

    $.Method(descriptor, methodName, makeSignature($.Single, true), from.float);
    $.Method(descriptor, methodName, makeSignature($.Double, true), from.float);

    $.Method(descriptor, methodName, makeSignature($.String), from.string);

    $.Method(descriptor, methodName, makeSignature($.String, true), from.string);
  };

  makeConvertMethods("Boolean", $.Boolean, {
    boolean: returnSame,
    uint: makeAdapter(Boolean),
    int: makeAdapter(Boolean),
    float: makeAdapter(Boolean),
    string: makeAdapter($jsilcore.$ParseBoolean)
  });

  makeConvertMethods("Byte", $.Byte, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("SByte", $.SByte, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("UInt16", $.UInt16, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("Int16", $.Int16, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("UInt32", $.UInt32, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("Int32", $.Int32, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("UInt64", $.UInt64, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("Int64", $.Int64, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseInt)
  });
  
  makeConvertMethods("Single", $.Single, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseFloat)
  });

  makeConvertMethods("Double", $.Double, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    string: makeAdapter($jsilcore.$ParseFloat)
  });

  makeConvertMethods("String", $.String, {
    boolean: boolToString,
    uint: makeAdapter(String),
    int: makeAdapter(String),
    float: makeAdapter(String),
    string: returnSame
  });

  var base64Table = [
    'A', 'B', 'C', 'D',
    'E', 'F', 'G', 'H',
    'I', 'J', 'K', 'L',
    'M', 'N', 'O', 'P',
    'Q', 'R', 'S', 'T',
    'U', 'V', 'W', 'X',
    'Y', 'Z',
    'a', 'b', 'c', 'd',
    'e', 'f', 'g', 'h',
    'i', 'j', 'k', 'l',
    'm', 'n', 'o', 'p',
    'q', 'r', 's', 't',
    'u', 'v', 'w', 'x',
    'y', 'z',
    '0', '1', '2', '3',
    '4', '5', '6', '7',
    '8', '9', 
    '+', '/'
  ];

  var base64CodeTable = new Array(base64Table.length);
  for (var i = 0; i < base64Table.length; i++)
    base64CodeTable[i] = base64Table[i].charCodeAt(0);

  var toBase64StringImpl = function ToBase64String (inArray, offset, length, options) {
    if (options)
      throw new Error("Base64FormattingOptions not implemented");

    var reader = $jsilcore.makeByteReader(inArray, offset, length);
    var result = "";
    var ch1 = 0, ch2 = 0, ch3 = 0, bits = 0, equalsCount = 0, sum = 0;
    var mask1 = (1 << 24) - 1, mask2 = (1 << 18) - 1, mask3 = (1 << 12) - 1, mask4 = (1 << 6) - 1;
    var shift1 = 18, shift2 = 12, shift3 = 6, shift4 = 0;

    while (true) {
      ch1 = reader.read();
      ch2 = reader.read();
      ch3 = reader.read();

      if (ch1 === false)
        break;
      if (ch2 === false) {
        ch2 = 0;
        equalsCount += 1;
      }
      if (ch3 === false) {
        ch3 = 0;
        equalsCount += 1;
      }

      // Seems backwards, but is right!
      sum = (ch1 << 16) | (ch2 << 8) | (ch3 << 0);

      bits = (sum & mask1) >> shift1;
      result += base64Table[bits];
      bits = (sum & mask2) >> shift2;
      result += base64Table[bits];

      if (equalsCount < 2) {
        bits = (sum & mask3) >> shift3;
        result += base64Table[bits];
      }

      if (equalsCount === 2) {
        result += "==";
      } else if (equalsCount === 1) {
        result += "=";
      } else {
        bits = (sum & mask4) >> shift4;
        result += base64Table[bits];
      }
    }

    return result;
  };

  $.Method({Static:true , Public:true }, "ToBase64String", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function ToBase64String (inArray) {
      return toBase64StringImpl(inArray, 0, inArray.length, 0);
    }
  );

  $.Method({Static:true , Public:true }, "ToBase64String", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte]), $jsilcore.TypeRef("System.Base64FormattingOptions")], [])), 
    function ToBase64String (inArray, options) {
      return toBase64StringImpl(inArray, 0, inArray.length, options);
    }
  );

  $.Method({Static:true , Public:true }, "ToBase64String", 
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function ToBase64String (inArray, offset, length) {
      return toBase64StringImpl(inArray, offset, length, 0);
    }
  );

  $.Method({Static:true , Public:true }, "ToBase64String", 
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32, $jsilcore.TypeRef("System.Base64FormattingOptions")
        ], [])), 
    toBase64StringImpl
  );

  $.Method({Static:true , Public:true }, "FromBase64String", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], [])), 
    function FromBase64String (s) {
      var lengthErrorMessage = "Invalid length for a Base-64 char array.";
      var contentErrorMessage = "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or a non-white space character among the padding characters.";

      var result = [];
      var reader = $jsilcore.makeCharacterReader(s);
      var sum = 0;
      var ch0 = 0, ch1 = 0, ch2 = 0, ch3 = 0;
      var index0 = -1, index1 = -1, index2 = -1, index3 = -1;
      var equals = "=".charCodeAt(0);

      while (true) {
        ch0 = reader.read();
        ch1 = reader.read();
        ch2 = reader.read();
        ch3 = reader.read();

        if (ch0 === false)
          break;
        if ((ch1 === false) || (ch2 === false) || (ch3 === false))
          throw new System.FormatException(lengthErrorMessage);

        index0 = base64CodeTable.indexOf(ch0);
        index1 = base64CodeTable.indexOf(ch1);
        index2 = base64CodeTable.indexOf(ch2);
        index3 = base64CodeTable.indexOf(ch3);

        if (
          (index0 < 0) || (index0 > 63) ||
          (index1 < 0) || (index1 > 63)
        )
          throw new System.FormatException(contentErrorMessage);

        sum = (index0 << 18) | (index1 << 12);

        if (index2 >= 0)
          sum |= (index2 << 6);
        else if (ch2 !== equals)
          throw new System.FormatException(contentErrorMessage);

        if (index3 >= 0)
          sum |= (index3 << 0);
        else if (ch3 !== equals)
          throw new System.FormatException(contentErrorMessage);

        result.push((sum >> 16) & 0xFF);
        if (index2 >= 0)
          result.push((sum >> 8) & 0xFF);
        if (index3 >= 0)
          result.push(sum & 0xFF);
      }

      return JSIL.Array.New($jsilcore.System.Byte, result);
    }
  );
});

JSIL.MakeStaticClass("System.Convert", true, [], function ($) {
});


$jsilcore.BytesFromBoolean = function (value) {
  return [value ? 1 : 0];
};

$jsilcore.BytesFromInt16 = function (value) {
  return [
    (value >> 0) & 0xFF,
    (value >> 8) & 0xFF
  ];
};

$jsilcore.BytesFromInt32 = function (value) {
  return [
    (value >> 0) & 0xFF,
    (value >> 8) & 0xFF,
    (value >> 16) & 0xFF,
    (value >> 24) & 0xFF
  ];
};

$jsilcore.BytesFromInt64 = function (value) {
  return [
    (value >> 0) & 0xFF,
    (value >> 8) & 0xFF,
    (value >> 16) & 0xFF,
    (value >> 24) & 0xFF,
    (value >> 32) & 0xFF,
    (value >> 40) & 0xFF,
    (value >> 48) & 0xFF,
    (value >> 56) & 0xFF
  ];
};

// FIXME: Are these unsigned versions right?

$jsilcore.BytesFromUInt16 = function (value) {
  return [
    (value >>> 0) & 0xFF,
    (value >>> 8) & 0xFF
  ];
};

$jsilcore.BytesFromUInt32 = function (value) {
  return [
    (value >>> 0) & 0xFF,
    (value >>> 8) & 0xFF,
    (value >>> 16) & 0xFF,
    (value >>> 24) & 0xFF
  ];
};

$jsilcore.BytesFromUInt64 = function (value) {
  return [
    (value >>> 0) & 0xFF,
    (value >>> 8) & 0xFF,
    (value >>> 16) & 0xFF,
    (value >>> 24) & 0xFF,
    (value >>> 32) & 0xFF,
    (value >>> 40) & 0xFF,
    (value >>> 48) & 0xFF,
    (value >>> 56) & 0xFF
  ];
};


$jsilcore.BytesToBoolean = function (bytes, offset) {
  return bytes[offset] !== 0;
};

$jsilcore.BytesToInt16 = function (bytes, offset) {
  var value = $jsilcore.BytesToUInt16(bytes, offset);
  if (value > 32767)
    return value - 65536;
  else
    return value;
};

$jsilcore.BytesToInt32 = function (bytes, offset) {
  var value = $jsilcore.BytesToUInt32(bytes, offset);
  if (value > 2147483647)
    return value - 4294967296;
  else
    return value;
};

$jsilcore.BytesToInt64 = function (bytes, offset) {
  // FIXME: Does this work right for negative numbers or does 53-bit rounding kill it?
  // FIXME: Generate warnings for values out of 53-bit range.
  var value = $jsilcore.BytesToUInt64(bytes, offset);
  if (value > System.Int64.MaxValue)
    return value - 18446744073709551616;
  else
    return value;
};

$jsilcore.BytesToUInt16 = function (bytes, offset) {
  return bytes[offset] + (bytes[offset + 1] * 256);
};

$jsilcore.BytesToUInt32 = function (bytes, offset) {
  return bytes[offset] + 
    (bytes[offset + 1] * 256) + 
    (bytes[offset + 2] * 65536) + 
    (bytes[offset + 3] * 16777216);
};

$jsilcore.BytesToUInt64 = function (bytes, offset) {
  // FIXME: Generate warnings for values out of 53-bit range.
  return bytes[offset] + 
    (bytes[offset + 1] << 8) + 
    (bytes[offset + 2] << 16) + 
    (bytes[offset + 3] << 24) + 
    (bytes[offset + 4] << 32) + 
    (bytes[offset + 5] << 40) + 
    (bytes[offset + 6] << 48) + 
    (bytes[offset + 7] << 56);
};

JSIL.ImplementExternals("System.BitConverter", function ($) {


  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Boolean], [])), 
    $jsilcore.BytesFromBoolean
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int16], [])), 
    $jsilcore.BytesFromInt16
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int32], [])), 
    $jsilcore.BytesFromInt32
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int64], [])), 
    $jsilcore.BytesFromInt64
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt16], [])), 
    $jsilcore.BytesFromUInt16
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt32], [])), 
    $jsilcore.BytesFromUInt32
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt64], [])), 
    $jsilcore.BytesFromUInt64
  );  

  /*
  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Single], [])), 
    $jsilcore.BytesFromSingle
  );

  $.Method({Static:true , Public:true }, "GetBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Double], [])), 
    $jsilcore.BytesFromDouble
  );

  $.Method({Static:true , Public:false}, "GetHexValue", 
    (new JSIL.MethodSignature($.Char, [$.Int32], [])), 
    function GetHexValue (i) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Int64BitsToDouble", 
    (new JSIL.MethodSignature($.Double, [$.Int64], [])), 
    function Int64BitsToDouble (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToChar", 
    (new JSIL.MethodSignature($.Char, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToChar (value, startIndex) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToDouble", 
    (new JSIL.MethodSignature($.Double, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToDouble (value, startIndex) {
      throw new Error('Not implemented');
    }
  );
  */

  $.Method({Static:true , Public:true }, "ToBoolean", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToBoolean
  );

  $.Method({Static:true , Public:true }, "ToInt16", 
    (new JSIL.MethodSignature($.Int16, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToInt16
  );

  $.Method({Static:true , Public:true }, "ToInt32", 
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToInt32
  );

  $.Method({Static:true , Public:true }, "ToInt64", 
    (new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToInt64
  );

  /*

  $.Method({Static:true , Public:true }, "ToSingle", 
    (new JSIL.MethodSignature($.Single, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToSingle (value, startIndex) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function ToString (value, startIndex, length) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function ToString (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToString (value, startIndex) {
      throw new Error('Not implemented');
    }
  );

  */

  $.Method({Static:true , Public:true }, "ToUInt16", 
    (new JSIL.MethodSignature($.UInt16, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToUInt16
  );

  $.Method({Static:true , Public:true }, "ToUInt32", 
    (new JSIL.MethodSignature($.UInt32, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToUInt32
  );

  $.Method({Static:true , Public:true }, "ToUInt64", 
    (new JSIL.MethodSignature($.UInt64, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    $jsilcore.BytesToUInt64
  );

});

JSIL.MakeStaticClass("System.BitConverter", true, [], function ($) {
});


//
//  Since alot of operators are shared between Int64 and UInt64, we construct both types using this function
JSIL.Make64BitInt = function ($, ctor, me) {

  var mscorlib = JSIL.GetCorlib();

  function lazy(f) {
      var state = null;
      return function () {
          if (state === null)
              state = f();
          return state;
      };
  };

  var maxValue = lazy(function () {
      return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
  });

  var zero = lazy(function () {
      return ctor(0, 0, 0);
  });

  var one = lazy(function () {
      return ctor(1, 0, 0);
  });

  var tryParse =
    function (text, style, result) {
      var r = zero();

      var radix = 10;

      if (style & System.Globalization.NumberStyles.AllowHexSpecifier)
        radix = 16;

      var rdx = ctor(radix, 0, 0);
      var neg = false;

      for (var i = 0; i < text.length; i++) {
        if (i == 0 && text[i] == '-') {
          neg = true;
          continue;
        }
        var c = parseInt(text[i], radix);
        if (isNaN(c)) {
          result.value = zero();
          return false;
        }
        r = me().op_Addition(ctor(c, 0, 0), me().op_Multiplication(rdx, r));
      }

      if (neg)
        r = me().op_UnaryNegation(r);

      result.value = r;

      return true;
    };

  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature($.Type, ["System.String"], [])),
    function (text) {
      var result = { value: null };
      if (!tryParse(text, 0, result))
        throw new System.Exception("NumberParseException");

      return result.value;
    });

  $.RawMethod({ Static: true, Public: true }, "Create",
        //(new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32")], [])),
        ctor);

  $.Method({ Static: true, Public: true }, "op_Addition",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          var ca = ad[0] + bd[0];
          var ra = (ca & 0xffffff000000) >> 24;
          var cb = ra + ad[1] + bd[1];
          var rb = (cb & 0xffffff000000) >> 24;
          var cc = rb + ad[2] + bd[2];
          return ctor(ca & 0xffffff, cb & 0xffffff, cc & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_Subtraction",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          var ca = ad[0] - bd[0];
          var ra = 0;
          if (ca < 0) {
            ca = 0x1000000 + ca;
            ra = -1;
          }
          var cb = ra + ad[1] - bd[1];
          var rb = 0;
          if (cb < 0) {
            cb = 0x1000000 + cb;
            rb = -1;
          }
          var cc = rb + ad[2] - bd[2];
          if (cc < 0) {
            cc = 0x10000 + cc;
          }
          return ctor(ca & 0xffffff, cb & 0xffffff, cc & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_LeftShift",
        (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
         function (a, n) { // a is UInt64, n is Int32
           if (n < 0) {
             return me().op_RightShift(a, -n);
           }

           if (n > 24) {
             return me().op_LeftShift(me().op_LeftShift(a, 24), n - 24);
           }

           var ad = a.data;
           var bat = ad[0] << n;
           var ba = bat & 0xffffff;
           var ra = (bat >>> 24) & 0xffffff;
           var bbt = (ad[1] << n) | ra;
           var bb = bbt & 0xffffff;
           var rb = (bbt >>> 24) & 0xffff;
           var bct = ad[2] << n;
           var bc = (bct & 0xffff) | rb;
           return ctor(ba, bb, bc);
         });

  $.Method({ Static: true, Public: true }, "op_OnesComplement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          return ctor((~ad[0]) & 0xffffff, (~ad[1]) & 0xffffff, (~ad[2]) & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_ExclusiveOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] ^ bd[0], ad[1] ^ bd[1], ad[2] ^ bd[2]);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseAnd",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] & bd[0], ad[1] & bd[1], ad[2] & bd[2]);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] | bd[0], ad[1] | bd[1], ad[2] | bd[2]);
        });


  $.Method({ Static: true, Public: true }, "op_Equality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ad[0] === bd[0] && ad[1] === bd[1] && ad[2] === bd[2];
        });

  $.Method({ Static: true, Public: true }, "op_Inequality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ad[0] !== bd[0] || ad[1] !== bd[1] || ad[2] !== bd[2];
        });

  $.Method({ Static: true, Public: true }, "op_Decrement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          if (ad[0] > 0)
            return ctor(ad[0] - 1, ad[1], ad[2]);
          else
            return me().op_Subtraction(a, one());
        });

  $.Method({ Static: true, Public: true }, "op_Increment",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          if (ad[0] < 0xffffff)
            return ctor(ad[0] + 1, ad[1], ad[2]);
          else
            return me().op_Addition(a, one());
        });

  $.Method({ Static: true, Public: true }, "op_Multiplication",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var s = zero();

          if (me().op_Equality(a, s) || me().op_Equality(b, s))
            return s;

          if (mscorlib.System.UInt64.op_GreaterThan(a, b))
            return me().op_Multiplication(b, a);


          if (a.data[0] & 1 == 1)
            s = b;

          var l = one();

          while (!me().op_Equality(a, l)) {
            a = mscorlib.System.UInt64.op_RightShift(a, 1);
            b = me().op_LeftShift(b, 1);

            if (a.data[0] & 1 == 1)
              s = me().op_Addition(b, s);
          }

          return s;
        });

  $.RawMethod(true, "CheckType", function (value) {
    return (typeof value.data === "object" && value.data.length == 3);
  });

};

JSIL.ImplementExternals("System.UInt64", function ($) {
    var ctor =
        function (a, b, c) {
            var r = new mscorlib.System.UInt64();
            r.data = [a, b, c];
            return r;
        };

    var mscorlib = JSIL.GetCorlib();

    var me = lazy(function () {
        return mscorlib.System.UInt64;
    });

    JSIL.Make64BitInt($, ctor, me);

    function lazy(f) {
        var state = null;
        return function () {
            if (state === null)
                state = f();
            return state;
        };
    };

    var maxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var minValue = lazy(function () {
        return ctor(0, 0, 0);
    });

    var one = lazy(function () {
        return ctor(1, 0, 0);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var q = ctor(0, 0, 0); // do not use UInt64.MinValue, we are mutating q !!
        var r = ctor(0, 0, 0);
        var nd = n.data;

        for (var i = 63; i >= 0; i--) {
            r = me().op_LeftShift(r, 1);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var s = (i - 24 * li);

            r.data[0] |= (nd[li] & (1 << s)) >>> s;

            if (me().op_GreaterThanOrEqual(r, d)) {
                r = me().op_Subtraction(r, d);
                q.data[li] |= 1 << s;
            }
        }

        return q;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var r = ctor(0, 0, 0);
        var nd = n.data;

        for (var i = 63; i >= 0; i--) {
            r = me().op_LeftShift(r, 1);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var s = (i - 24 * li);

            r.data[0] |= (nd[li] & (1 << s)) >>> s;

            if (me().op_Equality(r, d) || me().op_GreaterThan(r, d)) {
                r = me().op_Subtraction(r, d);
            }
        }

        return r;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
      (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
      function (a, n) {
          if (n < 0) {
              return me().op_LeftShift(a, -n);
          }

          n = n & 0x3f;

          if (n > 24) {
              return mscorlib.System.UInt64.op_RightShift(mscorlib.System.UInt64.op_RightShift(a, 24), n - 24);
          }

          var m = (1 << n) - 1;
          var ad = a.data;
          var cr = (ad[2] & m) << (24 - n);
          var ct = ad[2] >>> n;
          var br = (ad[1] & m) << (24 - n);
          var bt = ad[1] >>> n;
          var at = ad[0] >>> n;
          return ctor(at | br, bt | cr, ct);
      });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return ad[0] < bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return ad[0] <= bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return ad[0] > bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return ad[0] >= bd[0];
    });

    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function () {
        var a = this;
        var ten = me().FromNumber(10);

        var s = "";

        do {
            var r = me().op_Modulus(a, ten);
            s = r.data[0].toString() + s;
            a = me().op_Division(a, ten);
        } while (me().op_GreaterThan(a, minValue()));

        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToHex",
    new JSIL.MethodSignature("System.String", []),
    function () {

        var d = this.data;
        var s = d[0].toString(16);

        if (d[1] > 0 || d[2] > 0) {
            if (s.length < 6)
                s = (new Array(6 - s.length + 1)).join('0') + s;

            s = d[1].toString(16) + s;

            if (d[2] > 0) {
                if (s.length < 12)
                    s = (new Array(12 - s.length + 1)).join('0') + s;

                s = d[2].toString(16) + s;
            }
        }

        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToInt64",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int64"), []),
    function () {
        var d = this.data;
        return mscorlib.System.Int64.Create(d[0], d[1], d[2]);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "Clone",
    new JSIL.MethodSignature($.Type, []),
    function () {
        var d = this.data;
        return ctor(d[0], d[1], d[2]);
    });

    $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function (rhs) {
        return UInt64.op_Equality(this, rhs);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Int32")], [])),
    function (n) {
        if (n < 0)
            throw new Error("cannot construct UInt64 from negative number");

        // only using 48 bits

        var n0 = Math.floor(n);
        return ctor(
            (n0 & 0xffffff),
            (n0 >>> 24) & 0xffffff,
            0);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Double"), [], [])),
    function () {
        return 0x1000000 * (0x1000000 * this.data[2] + this.data[1]) + this.data[0];
    });
});

JSIL.ImplementExternals("System.Int64", function ($) {

    // The unsigned range 0 to 0x7FFFFFFFFFFFFFFF (= Int64.MaxValue) is positive: 0 to 9223372036854775807
    // The directly following unsigned range 0x8000000000000000 (= Int64.MaxValue + 1 = Int64.MinValue) to 0xFFFFFFFFFFFFFFFF is negative: -9223372036854775808 to -1
    //
    //  signed value
    //  ^
    //  |      /
    //  |    /
    //  |  /
    //  |/z
    //  ------------------> unsigned value
    //  |              /
    //  |            /
    //  |          /
    //  |        /
    //

    var ctor =
      function (a, b, c) {
          var r = new mscorlib.System.Int64();
          r.data = [a, b, c];
          return r;
      };

    var mscorlib = JSIL.GetCorlib();
    
    function lazy(f) {
        var state = null;
        return function () {
            if (state === null)
                state = f();
            return state;
        };
    };

    var me = lazy(function () {
        return mscorlib.System.Int64;
    });

    JSIL.Make64BitInt($, ctor, me);

    var zero = lazy(function () {
        return ctor(0, 0, 0);
    });

    var one = lazy(function () {
        return ctor(1, 0, 0);
    });

    var minusOne = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var signedMaxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0x7FFF);
    });

    var unsignedMaxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var isNegative = function (a) {
        return mscorlib.System.UInt64.op_GreaterThan(a, signedMaxValue());
    };

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_UnaryNegation",
    (new JSIL.MethodSignature($.Type, [$.Type], [])),
    function (a) {
        return me().op_Addition(me().op_Subtraction(unsignedMaxValue(), a), one());
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (isNegative(d))
            return me().op_Division(me().op_UnaryNegation(n), me().op_UnaryNegation(d));
        else if (isNegative(n))
            return me().op_UnaryNegation(me().op_Division(me().op_UnaryNegation(n), d));
        else
        // fix return type error
            return mscorlib.System.UInt64.op_Division(n, d).ToInt64();
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (isNegative(d))
            return me().op_Modulus(me().op_UnaryNegation(n), me().op_UnaryNegation(d));
        else if (isNegative(n))
            return me().op_UnaryNegation(me().op_Modulus(me().op_UnaryNegation(n), d));
        else
        // fix return type error
            return mscorlib.System.UInt64.op_Modulus(n, d);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
    (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
    function (a, n) {
        // Int64 (signed) uses arithmetic shift, UIn64 (unsigned) uses logical shift

        if (n < 0) {
            return me().op_LeftShift(a, -n);
        }

        if (isNegative(a)) {
            return me().op_UnaryNegation(mscorlib.System.UInt64.op_RightShift(a, n));
        }
        else {
            return mscorlib.System.UInt64.op_RightShift(a, n).ToInt64();
        }
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_GreaterThan(a, b);
        else
            return bn;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_GreaterThanOrEqual(a, b);
        else
            return bn;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function Int64_op_LessThan(a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_LessThan(a, b);
        else
            return an;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_LessThanOrEqual(a, b);
        else
            return an;
    });

    // Might need to be implemented externally
    $.Method({ Static: false, Public: true }, "Equals",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), []),
    function (a) {
        return me().op_Equality(this, a);
    });

    // Might need to be implemented externally
    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function () {
        var s = "";
        var a = this;
        if (isNegative(this)) {
            s += "-";
            a = me().op_UnaryNegation(this);
        }
        s += mscorlib.System.UInt64.prototype.toString.apply(a);
        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Int32")], [])),
    function (n) {
        var sign = n < 0 ? -1 : 1;
        n = Math.abs(n);

        var n0 = Math.floor(n);
        var r = ctor(n0 & 0xffffff, (n0 >>> 24) & 0xffffff, 0);

        if (sign == -1)
            return me().op_UnaryNegation(r);
        else
            return r;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToUInt64",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.UInt64"), []),
    function () {
        var d = this.data;
        return mscorlib.System.UInt64.Create(d[0], d[1], d[2]);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Double"), [], [])),
    function () {
        var neg = isNegative(this);
        var n = neg ? me().op_UnaryNegation(this) : this;
        var r = 0x1000000 * (0x1000000 * n.data[2] + n.data[1]) + n.data[0]; ;

        if (neg)
            return -r;
        else
            return r;
    });
});

JSIL.MakeStruct("System.ValueType", "System.UInt64", true, [], function ($) {
    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );
});

JSIL.MakeStruct("System.ValueType", "System.Int64", true, [], function ($) {
    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );
});
