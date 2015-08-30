JSIL.ImplementExternals("System.Text.UnicodeEncoding", function ($) {
  var writePair = function (writer, a, b) {
    writer.write(a);
    writer.write(b);
  };

  var readPair = function (reader) {
    var a = reader.read();
    var b = reader.read();

    if ((a === false) || (b === false))
      return false;

    return (a << 8) | b;
  };

  var swapBytes = function (word) {
    return ((word & 0xFF) << 8) | ((word >> 8) & 0xFF);
  };

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.bigEndian = false;
      this.emitBOM = true;
      this.throwOnInvalid = false;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Boolean], [])),
    function _ctor(bigEndian) {
      this.bigEndian = bigEndian;
      this.emitBOM = true;
      this.throwOnInvalid = false;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Boolean, $.Boolean], [])),
    function _ctor(bigEndian, byteOrderMark) {
      this.bigEndian = bigEndian;
      this.emitBOM = byteOrderMark;
      this.throwOnInvalid = false;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Boolean, $.Boolean, $.Boolean], [])),
    function _ctor(bigEndian, byteOrderMark, throwOnInvalidBytes) {
      this.bigEndian = bigEndian;
      this.emitBOM = byteOrderMark;
      this.throwOnInvalid = throwOnInvalidBytes;
    }
  );

  $.RawMethod(false, "$encode", function UnicodeEncoding_Encode(string, outputBytes, outputIndex) {
    var writer = this.$makeWriter(outputBytes, outputIndex);
    var reader = this.$makeCharacterReader(string), ch, lowBits, highBits;

    var hasError = false;

    while (!reader.eof) {
      ch = reader.read();

      if (ch === false)
        continue;

      if (ch < 0x10000) {
        if (this.bigEndian)
          ch = swapBytes(ch);

        writePair(writer, ch & 0xFF, (ch >> 8) & 0xFF);
      } else if (ch <= 0x10FFFF) {
        ch -= 0x10000;

        if (this.bigEndian)
          ch = swapBytes(ch);

        lowBits = (ch & 0x3FF) | 0xDC00;
        highBits = ((ch >> 10) & 0x3FF) | 0xD800;

        writePair(writer, highBits & 0xFF, (highBits >> 8) & 0xFF);
        writePair(writer, lowBits & 0xFF, (lowBits >> 8) & 0xFF);
      } else {
        hasError = true;
      }
    }

    return writer.getResult();
  });

  $.RawMethod(false, "$decode", function UnicodeEncoding_Decode(bytes, index, count) {
    var reader = this.$makeByteReader(bytes, index, count);
    var result = "";
    var hasError;
    var firstWord, secondWord, charCode;

    while (!reader.eof) {
      firstWord = readPair(reader);

      if (firstWord === false)
        continue;

      if ((firstWord < 0xD800) || (firstWord > 0xDFFF)) {
        charCode = firstWord;

        if (!this.bigEndian)
          charCode = swapBytes(charCode);

        result += this.$fromCharCode(charCode);
        hasError = false;
      } else if ((firstWord >= 0xD800) && (firstWord <= 0xDBFF)) {
        secondWord = readPair(reader);
        if (secondWord === false) {
          hasError = true;
        } else {
          var highBits = firstWord & 0x3FF;
          var lowBits = secondWord & 0x3FF;
          charCode = ((highBits << 10) | lowBits) + 0x10000;

          if (!this.bigEndian)
            charCode = swapBytes(charCode);

          result += this.$fromCharCode(charCode);
        }
      } else {
        hasError = true;
      }

      if (hasError) {
        if (this.throwOnInvalid)
          JSIL.RuntimeError("Invalid character in UTF16 text");
        else
          result += this.fallbackCharacter;
      }
    }

    return result;
  });
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.UnicodeEncoding", true, [], function ($) {
});