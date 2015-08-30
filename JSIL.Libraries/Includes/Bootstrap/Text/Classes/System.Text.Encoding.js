//? include("../Utils/$jsilcore.fromCharCode.js");
//? include("../Utils/$jsilcore.charCodeAt.js");

JSIL.ImplementExternals("System.Text.Encoding", function ($) {
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
            JSIL.RuntimeError("End of buffer");

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

  $.RawMethod(false, "$encode", function Encoding_Encode_PureVirtual(string, outputBytes, outputIndex) {
    throw new Error("Not implemented");
  });

  $.RawMethod(false, "$decode", function Encoding_Decode_PureVirtual(bytes, index, count) {
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

  $.Method({ Static: true, Public: true }, "get_ASCII",
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      if (!System.Text.Encoding.asciiEncoding)
        System.Text.Encoding.asciiEncoding = JSIL.CreateInstanceOfType(
          System.Text.ASCIIEncoding.__Type__, "$fromCharset", ["US-ASCII"]
        );

      return System.Text.Encoding.asciiEncoding;
    }
  );

  $.Method({ Static: true, Public: true }, "get_UTF8",
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      if (!System.Text.Encoding.utf8Encoding)
        System.Text.Encoding.utf8Encoding = JSIL.CreateInstanceOfType(
          System.Text.UTF8Encoding.__Type__, "$fromCharset", ["UTF-8"]
        );

      return System.Text.Encoding.utf8Encoding;
    }
  );

  $.Method({ Static: true, Public: true }, "get_UTF7",
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      if (!System.Text.Encoding.utf7Encoding)
        System.Text.Encoding.utf7Encoding = JSIL.CreateInstanceOfType(
          System.Text.UTF7Encoding.__Type__, "$fromCharset", ["UTF-7"]
        );

      return System.Text.Encoding.utf7Encoding;
    }
  );

  $.Method({ Static: true, Public: true }, "get_Unicode",
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      if (!System.Text.Encoding.unicodeEncoding)
        System.Text.Encoding.unicodeEncoding = new $jsilcore.System.Text.UnicodeEncoding(false, true);

      return System.Text.Encoding.unicodeEncoding;
    }
  );

  $.Method({ Static: true, Public: true }, "get_BigEndianUnicode",
    (new JSIL.MethodSignature($.Type, [], [])),
    function () {
      if (!System.Text.Encoding.bigEndianUnicodeEncoding)
        System.Text.Encoding.bigEndianUnicodeEncoding = new $jsilcore.System.Text.UnicodeEncoding(true, true);

      return System.Text.Encoding.bigEndianUnicodeEncoding;
    }
  );

  $.Method({ Static: false, Public: true }, "GetByteCount",
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Char])], [])),
    function GetByteCount(chars) {
      return this.$encode(this.$charsToString(chars)).length;
    }
  );

  $.Method({ Static: false, Public: true }, "GetByteCount",
    (new JSIL.MethodSignature($.Int32, [$.String], [])),
    function GetByteCount(s) {
      return this.$encode(s).length;
    }
  );

  $.Method({ Static: false, Public: true }, "GetByteCount",
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
          $.Int32
    ], [])),
    function GetByteCount(chars, index, count) {
      return this.$encode(this.$charsToString(chars, index, count)).length;
    }
  );

  $.Method({ Static: false, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$jsilcore.TypeRef("System.Array", [$.Char])], [])),
    function GetBytes(chars) {
      return this.$encode(this.$charsToString(chars));
    }
  );

  $.Method({ Static: false, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
          $.Int32
    ], [])),
    function GetBytes(chars, index, count) {
      return this.$encode(this.$charsToString(chars, index, count));
    }
  );

  $.Method({ Static: false, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Byte]),
          $.Int32
    ], [])),
    function GetBytes(chars, charIndex, charCount, bytes, byteIndex) {
      return this.$encode(this.$charsToString(chars, index, count), bytes, byteIndex);
    }
  );

  $.Method({ Static: false, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], [])),
    function GetBytes(s) {
      return this.$encode(s);
    }
  );

  $.Method({ Static: false, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($.Int32, [
          $.String, $.Int32,
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Byte]),
          $.Int32
    ], [])),
    function GetBytes(s, charIndex, charCount, bytes, byteIndex) {
      return this.$encode(s.substr(charIndex, charCount), bytes, byteIndex);
    }
  );

  $.Method({ Static: false, Public: true }, "GetCharCount",
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
    function GetCharCount(bytes) {
      return this.$decode(bytes).length;
    }
  );

  $.Method({ Static: false, Public: true }, "GetCharCount",
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function GetCharCount(bytes, index, count) {
      return this.$decode(bytes, index, count).length;
    }
  );

  $.Method({ Static: false, Public: true }, "GetChars",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
    function GetChars(bytes) {
      return this.$stringToChars(this.$decode(bytes));
    }
  );

  $.Method({ Static: false, Public: true }, "GetChars",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function GetChars(bytes, index, count) {
      return this.$stringToChars(this.$decode(bytes, index, count));
    }
  );

  $.Method({ Static: false, Public: true }, "GetChars",
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32, $jsilcore.TypeRef("System.Array", [$.Char]),
          $.Int32
    ], [])),
    function GetChars(bytes, byteIndex, byteCount, chars, charIndex) {
      throw new Error("Not implemented");
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
    function GetString(bytes) {
      return this.$decode(bytes);
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function GetString(bytes, index, count) {
      return this.$decode(bytes, index, count);
    }
  );
});

JSIL.MakeClass("System.Object", "System.Text.Encoding", true, [], function ($) {
  $.Property({ Static: true, Public: true }, "ASCII");
  $.Property({ Static: true, Public: true }, "UTF8");
  $.Property({ Static: true, Public: true }, "UTF7");
  $.Property({ Static: true, Public: true }, "Unicode");
  $.Property({ Static: true, Public: true }, "BigEndianUnicode");
});