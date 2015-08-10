//? include("../Utils/$jsilcore.$ParseInt.js");
//? include("../Utils/$jsilcore.$ParseFloat.js");
//? include("../Utils/$jsilcore.$ParseBoolean.js");
JSIL.ImplementExternals("System.Convert", function ($) {
  $.Method({ Static: true, Public: true }, "ChangeType",
    (new JSIL.MethodSignature($.Object, [$.Object, $jsilcore.TypeRef("System.Type")], [])),
    function ChangeType(value, conversionType) {
      // FIXME: Actually compatible?
      if (value && $jsilcore.System.IConvertible.$Is(value)) {
        // FIXME: provider
        return $jsilcore.System.IConvertible.ToType.Call(value, null, conversionType, null);
      } else {
        return conversionType.__PublicInterface__.$As(value);
      }
    }
  );

  var returnSame = function (value) {
    return value;
  };

  var returnValueOf = function (value) {
    return value.valueOf();
  };

  var makeAdapter = function (adapter) {
    if (!adapter)
      JSIL.RuntimeError("No adapter provided");

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

    var descriptor = { Static: true, Public: true };
    var tFormatProvider = $jsilcore.TypeRef("System.IFormatProvider");

    var toType = JSIL.ResolveTypeReference(to)[0];

    var makeSignature = function (argType, formatProvider) {
      if (formatProvider)
        return new JSIL.MethodSignature(to, [argType, tFormatProvider], []);
      else
        return new JSIL.MethodSignature(to, [argType], []);
    };

    if (from.boolean) {
      $.Method(descriptor, methodName, makeSignature($.Boolean), from.boolean);

      $.Method(descriptor, methodName, makeSignature($.Boolean, true), from.boolean);
    }

    $.Method(descriptor, methodName, makeSignature($.SByte), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int16), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int32), from.int);

    $.Method(descriptor, methodName, makeSignature($.SByte, true), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int16, true), from.int);
    $.Method(descriptor, methodName, makeSignature($.Int32, true), from.int);

    $.Method(descriptor, methodName, makeSignature($.Byte), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt16), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt32), from.uint);

    $.Method(descriptor, methodName, makeSignature($.Byte, true), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt16, true), from.uint);
    $.Method(descriptor, methodName, makeSignature($.UInt32, true), from.uint);

    if (from.int64) {
      $.Method(descriptor, methodName, makeSignature($.Int64), from.int64);
      $.Method(descriptor, methodName, makeSignature($.Int64, true), from.int64);
    }

    if (from.uint64) {
      $.Method(descriptor, methodName, makeSignature($.UInt64), from.uint64);
      $.Method(descriptor, methodName, makeSignature($.UInt64, true), from.uint64);
    }

    if (from.float) {
      $.Method(descriptor, methodName, makeSignature($.Single), from.float);
      $.Method(descriptor, methodName, makeSignature($.Double), from.float);

      $.Method(descriptor, methodName, makeSignature($.Single, true), from.float);
      $.Method(descriptor, methodName, makeSignature($.Double, true), from.float);
    }

    if (from.string) {
      $.Method(descriptor, methodName, makeSignature($.String), from.string);

      $.Method(descriptor, methodName, makeSignature($.String, true), from.string);
    }

    var fromObject = function Convert_FromObject(value) {
      if (value === null) {
        if (to === $.String) {
          return $jsilcore.System.String.Empty;
        }

        return JSIL.DefaultValue(toType);
      }

      if ($jsilcore.System.IConvertible.$Is(value)) {
        var conversionMethod = $jsilcore.System.IConvertible["To" + typeName];

        if (conversionMethod) {
          return conversionMethod.Call(value, null, null);
        }
      }

      if (to === $.String) {
        return value.toString();
      }

      if ($jsilcore.System.String.$Is(value))
        return from.string(value);
      else if (from.int64 && $jsilcore.System.Int64.$Is(value))
        return from.int64(value);
      else if (from.uint64 && $jsilcore.System.UInt64.$Is(value))
        return from.uint64(value);
      else if ($jsilcore.System.Int32.$Is(value))
        return from.int(value);
      else if ($jsilcore.System.UInt32.$Is(value))
        return from.uint(value);
      else if ($jsilcore.System.Boolean.$Is(value))
        return from.boolean(value);
      else if ($jsilcore.System.Double.$Is(value))
        return from.float(value);
      else
        throw new System.NotImplementedException(
          "Conversion from type '" + JSIL.GetType(value) + "' to type '" + typeName + "' not implemented."
        );
    };

    $.Method(descriptor, methodName, makeSignature($.Object), fromObject);
    $.Method(descriptor, methodName, makeSignature($.Object, true), fromObject);
  };

  makeConvertMethods("Boolean", $.Boolean, {
    boolean: returnSame,
    uint: makeAdapter(Boolean),
    int: makeAdapter(Boolean),
    float: makeAdapter(Boolean),
    int64: makeAdapter(Boolean),
    uint64: makeAdapter(Boolean),
    string: makeAdapter($jsilcore.$ParseBoolean)
  });

  makeConvertMethods("Byte", $.Byte, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("SByte", $.SByte, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("UInt16", $.UInt16, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("Int16", $.Int16, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("UInt32", $.UInt32, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  makeConvertMethods("Int32", $.Int32, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseInt)
  });

  var boolToUInt64 = function (b) {
    return $jsilcore.System.UInt64.FromInt32(b ? 1 : 0);
  };

  var intToUInt64 = function (i) {
    return $jsilcore.System.UInt64.FromInt32(i);
  };

  var uintToUInt64 = function (u) {
    return $jsilcore.System.UInt64.FromUInt32(u);
  };

  var parseUInt64 = function (text) {
    return $jsilcore.System.UInt64.Parse(text);
  };

  makeConvertMethods("UInt64", $.UInt64, {
    boolean: boolToUInt64,
    uint: uintToUInt64,
    int: intToUInt64,
    string: parseUInt64
  });

  var boolToInt64 = function (b) {
    return $jsilcore.System.Int64.FromInt32(b ? 1 : 0);
  }

  var intToInt64 = function (i) {
    return $jsilcore.System.Int64.FromInt32(i);
  };

  var uintToInt64 = function (u) {
    return $jsilcore.System.Int64.FromUInt32(u);
  };

  var parseInt64 = function (text) {
    return $jsilcore.System.Int64.Parse(text);
  };

  var intToChar = function (i) {
    return String.fromCharCode(i | 0);
  };

  var valueOfToChar = function (obj) {
    return String.fromCharCode(obj.valueOf() | 0);
  };

  makeConvertMethods("Int64", $.Int64, {
    boolean: boolToInt64,
    uint: uintToInt64,
    int: intToInt64,
    string: parseInt64
  });

  makeConvertMethods("Single", $.Single, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseFloat)
  });

  makeConvertMethods("Double", $.Double, {
    boolean: boolToInt,
    uint: returnSame,
    int: returnSame,
    float: returnSame,
    int64: returnValueOf,
    uint64: returnValueOf,
    string: makeAdapter($jsilcore.$ParseFloat)
  });

  makeConvertMethods("Char", $.Char, {
    uint: intToChar,
    int: intToChar,
    int64: valueOfToChar,
    uint64: valueOfToChar
  });

  makeConvertMethods("String", $.String, {
    boolean: boolToString,
    uint: makeAdapter(String),
    int: makeAdapter(String),
    float: makeAdapter(String),
    int64: makeAdapter(String),
    uint64: makeAdapter(String),
    string: returnSame
  });

  var base64IgnoredCodepoints = [
    9, 10, 13, 32
  ];

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

  var toBase64StringImpl = function ToBase64String(inArray, offset, length, options) {
    if (options)
      JSIL.RuntimeError("Base64FormattingOptions not implemented");

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

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
    function ToBase64String(inArray) {
      return toBase64StringImpl(inArray, 0, inArray.length, 0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte]), $jsilcore.TypeRef("System.Base64FormattingOptions")], [])),
    function ToBase64String(inArray, options) {
      return toBase64StringImpl(inArray, 0, inArray.length, options);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function ToBase64String(inArray, offset, length) {
      return toBase64StringImpl(inArray, offset, length, 0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32, $jsilcore.TypeRef("System.Base64FormattingOptions")
    ], [])),
    toBase64StringImpl
  );

  $.Method({ Static: true, Public: true }, "FromBase64String",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], [])),
    function FromBase64String(s) {
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
        if (ch0 === false)
          break;
        if (base64IgnoredCodepoints.indexOf(ch0) >= 0)
          continue;

        ch1 = reader.read();
        ch2 = reader.read();
        ch3 = reader.read();

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

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Convert", true, [], function ($) {
});
//? }