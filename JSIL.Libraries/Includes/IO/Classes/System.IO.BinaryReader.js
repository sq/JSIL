JSIL.ImplementExternals("System.IO.BinaryReader", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])),
      function _ctor(input) {
          if (typeof (input) !== "object")
              throw new Error("Invalid stream");

          this.m_stream = input;
          this.m_encoding = $jsilcore.System.Text.Encoding.get_UTF8();
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])),
      function _ctor(input, encoding) {
          if (typeof (input) !== "object")
              throw new Error("Invalid stream");

          this.m_stream = input;
          this.m_encoding = encoding;
      }
    );

    $.Method({ Static: false, Public: true }, "Close",
      (JSIL.MethodSignature.Void),
      function Close() {
          this.m_stream = null;
          this.m_encoding = null;
      }
    );

    $.Method({ Static: false, Public: false }, "Read7BitEncodedInt",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function Read7BitEncodedInt() {
          var result = 0, bits = 0;

          while (bits < 35) {
              var b = this.ReadByte();
              result |= (b & 127) << bits;
              bits += 7;

              if ((b & 128) == 0)
                  return result;
          }

          throw new System.FormatException("Bad 7-bit int format");
      }
    );

    $.Method({ Static: false, Public: true }, "ReadBoolean",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function ReadBoolean() {
          return $jsilcore.BytesToBoolean(this.$readBytesTemp(1), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadByte",
      (new JSIL.MethodSignature($.Byte, [], [])),
      function ReadByte() {
          return this.m_stream.ReadByte();
      }
    );

    $.RawMethod(false, "$readBytesTemp", function (count) {
        if (!this.m_tempBuffer || (this.m_tempBuffer.length < count))
            this.m_tempBuffer = JSIL.Array.New(System.Byte, count);

        var bytesRead = this.m_stream.Read(this.m_tempBuffer, 0, count);
        if (bytesRead < count)
            throw new System.IO.EndOfStreamException();

        return this.m_tempBuffer;
    });

    $.Method({ Static: false, Public: true }, "ReadBytes",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int32], [])),
      function ReadBytes(count) {
          var result = JSIL.Array.New(System.Byte, count);
          var bytesRead = this.m_stream.Read(result, 0, count);

          if (bytesRead < count) {
              var oldArray = result;
              result = JSIL.Array.New(System.Byte, bytesRead);
              JSIL.Array.CopyTo(oldArray, result, 0);
          }

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "Read",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function Read() {
          return this.ReadChar().charCodeAt(0);
      }
    );

    $.Method({ Static: false, Public: true }, "Read",
      (new JSIL.MethodSignature($.Int32, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function Read(buffer, index, count) {
          this.m_stream.Read(buffer, index, count);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadChar",
      (new JSIL.MethodSignature($.Char, [], [])),
      function ReadChar() {
          var ch = $jsilio.ReadCharFromStream(this.m_stream, this.m_encoding);
          if (!ch)
              throw new System.Exception("End of stream");

          return ch;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadChars",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [$.Int32], [])),
      function ReadChars(count) {
          var result = new Array(count);

          for (var i = 0; i < count; i++) {
              var ch = $jsilio.ReadCharFromStream(this.m_stream, this.m_encoding);
              if (ch === -1)
                  return result.slice(0, i - 1);

              result[i] = ch;
          };

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadDouble",
      (new JSIL.MethodSignature($.Double, [], [])),
      function ReadDouble() {
          var bytes = this.ReadBytes(8);
          return this.$decodeFloat(bytes, 1, 11, 52, -1022, 1023, true);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadInt16",
      (new JSIL.MethodSignature($.Int16, [], [])),
      function ReadInt16() {
          return $jsilcore.BytesToInt16(this.$readBytesTemp(2), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadInt32",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function ReadInt32() {
          return $jsilcore.BytesToInt32(this.$readBytesTemp(4), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadInt64",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function ReadInt64() {
          return $jsilcore.BytesToInt64(this.$readBytesTemp(8), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadSByte",
      (new JSIL.MethodSignature($.SByte, [], [])),
      function ReadSByte() {
          var byt = this.m_stream.ReadByte();
          if (byt > 127)
              return byt - 256;
          else
              return byt;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadSingle",
      (new JSIL.MethodSignature($.Single, [], [])),
      function ReadSingle() {
          var bytes = this.ReadBytes(4);
          return this.$decodeFloat(bytes, 1, 8, 23, -126, 127, true);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadString() {
          var size = this.Read7BitEncodedInt();
          if (size <= 0)
              return "";

          var bytes = this.$readBytesTemp(size);
          var result = this.m_encoding.$decode(bytes, 0, size);
          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadUInt16",
      (new JSIL.MethodSignature($.UInt16, [], [])),
      function ReadUInt16() {
          return $jsilcore.BytesToUInt16(this.$readBytesTemp(2), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadUInt32",
      (new JSIL.MethodSignature($.UInt32, [], [])),
      function ReadUInt32() {
          return $jsilcore.BytesToUInt32(this.$readBytesTemp(4), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadUInt64",
      (new JSIL.MethodSignature($.UInt64, [], [])),
      function ReadUInt64() {
          return $jsilcore.BytesToUInt64(this.$readBytesTemp(8), 0);
      }
    );

    $.Method({ Static: false, Public: true }, "PeekChar",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function PeekChar() {
          var peeked = this.m_stream.$PeekByte();
          if (peeked === -1)
              return peeked;

          var position = this.m_stream.Position;
          var ch = this.ReadChar();
          this.m_stream.Position = position;

          if (ch)
              return ch.charCodeAt(0);
          else
              return -1;
      }
    );

    $.RawMethod(false, "$decodeFloat",
      // Derived from http://stackoverflow.com/a/8545403/106786
      function decodeFloat(bytes, signBits, exponentBits, fractionBits, eMin, eMax, littleEndian) {
          var totalBits = (signBits + exponentBits + fractionBits);

          var binary = "";
          for (var i = 0, l = bytes.length; i < l; i++) {
              var bits = bytes[i].toString(2);
              while (bits.length < 8)
                  bits = "0" + bits;

              if (littleEndian)
                  binary = bits + binary;
              else
                  binary += bits;
          }

          var sign = (binary.charAt(0) == '1') ? -1 : 1;
          var exponent = parseInt(binary.substr(signBits, exponentBits), 2) - eMax;
          var significandBase = binary.substr(signBits + exponentBits, fractionBits);
          var significandBin = '1' + significandBase;
          var i = 0;
          var val = 1;
          var significand = 0;

          if (exponent == -eMax) {
              if (significandBase.indexOf('1') == -1)
                  return 0;
              else {
                  exponent = eMin;
                  significandBin = '0' + significandBase;
              }
          }

          while (i < significandBin.length) {
              significand += val * parseInt(significandBin.charAt(i));
              val = val / 2;
              i++;
          }

          return sign * significand * Math.pow(2, exponent);
      }
    );

    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function Dispose() {
          this.m_stream = null;
      }
    );

    $.Method({ Static: false, Public: true }, "get_BaseStream",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [], [])),
      function get_BaseStream() {
          return this.m_stream;
      }
    );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.IO.BinaryReader", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "Close",
    JSIL.MethodSignature.Void
  );

  $.ExternalMethod({ Static: false, Public: false }, "Dispose",
    new JSIL.MethodSignature(null, [$.Boolean], [])
  );

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
});
//? }