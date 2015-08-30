JSIL.ImplementExternals("System.Text.UTF8Encoding", function ($) {
  var UTF8ByteSwapNotAChar = 0xFFFE;
  var UTF8NotAChar = 0xFFFF;

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.emitBOM = false;
      this.throwOnInvalid = false;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Boolean], [])),
    function _ctor(encoderShouldEmitUTF8Identifier) {
      this.emitBOM = encoderShouldEmitUTF8Identifier;
      this.throwOnInvalid = false;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Boolean, $.Boolean], [])),
    function _ctor(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes) {
      this.emitBOM = encoderShouldEmitUTF8Identifier;
      this.throwOnInvalid = throwOnInvalidBytes;
    }
  );

  $.RawMethod(false, "$encode", function UTF8Encoding_Encode(string, outputBytes, outputIndex) {
    // http://tidy.sourceforge.net/cgi-bin/lxr/source/src/utf8.c

    var writer = this.$makeWriter(outputBytes, outputIndex);
    var reader = this.$makeCharacterReader(string), ch;

    var hasError = false;

    while (!reader.eof) {
      ch = reader.read();

      if (ch === false)
        continue;

      if (ch <= 0x7F) {
        writer.write(ch);
      } else if (ch <= 0x7FF) {
        writer.write(0xC0 | (ch >> 6));
        writer.write(0x80 | (ch & 0x3F));
      } else if (ch <= 0xFFFF) {
        writer.write(0xE0 | (ch >> 12));
        writer.write(0x80 | ((ch >> 6) & 0x3F));
        writer.write(0x80 | (ch & 0x3F));
      } else if (ch <= 0x1FFFF) {
        writer.write(0xF0 | (ch >> 18));
        writer.write(0x80 | ((ch >> 12) & 0x3F));
        writer.write(0x80 | ((ch >> 6) & 0x3F));
        writer.write(0x80 | (ch & 0x3F));

        if ((ch === UTF8ByteSwapNotAChar) || (ch === UTF8NotAChar))
          hasError = true;
      } else if (ch <= 0x3FFFFFF) {
        writer.write(0xF0 | (ch >> 24));
        writer.write(0x80 | ((ch >> 18) & 0x3F));
        writer.write(0x80 | ((ch >> 12) & 0x3F));
        writer.write(0x80 | ((ch >> 6) & 0x3F));
        writer.write(0x80 | (ch & 0x3F));

        hasError = true;
      } else if (ch <= 0x7FFFFFFF) {
        writer.write(0xF0 | (ch >> 30));
        writer.write(0x80 | ((ch >> 24) & 0x3F));
        writer.write(0x80 | ((ch >> 18) & 0x3F));
        writer.write(0x80 | ((ch >> 12) & 0x3F));
        writer.write(0x80 | ((ch >> 6) & 0x3F));
        writer.write(0x80 | (ch & 0x3F));

        hasError = true;
      } else {
        hasError = true;
      }
    }

    return writer.getResult();
  });

  $.RawMethod(false, "$decode", function UTF8Encoding_Decode(bytes, index, count) {
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
          JSIL.RuntimeError("Invalid character in UTF8 text");
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