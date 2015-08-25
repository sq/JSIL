JSIL.ImplementExternals("System.IO.BinaryWriter", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])),
      function _ctor(output) {
          this.m_stream = output;
          this.m_encoding = new System.Text.UTF8Encoding(false, true);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])),
      function _ctor(output, encoding) {
          this.m_stream = output;
          this.m_encoding = encoding;
      }
    );

    $.Method({ Static: false, Public: true }, "get_BaseStream",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [], [])),
      function get_BaseStream() {
          return this.m_stream;
      }
    );

    $.RawMethod(false, "$writeBytes", function (bytes) {
        this.m_stream.Write(bytes, 0, bytes.length);
    });

    $.Method({ Static: false, Public: true }, "Flush",
      (JSIL.MethodSignature.Void),
      function Flush() {
      }
    );

    $.Method({ Static: false, Public: true }, "Seek",
      (new JSIL.MethodSignature($.Int64, [$.Int32, $jsilcore.TypeRef("System.IO.SeekOrigin")], [])),
      function Seek(offset, origin) {
          this.m_stream.Seek($jsilcore.System.Int64.FromInt32(offset), origin);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromBoolean(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Byte], [])),
      function Write(value) {
          this.$writeBytes([value]);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.SByte], [])),
      function Write(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
      function Write(buffer) {
          this.$writeBytes(buffer);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function Write(buffer, index, count) {
          this.m_stream.Write(buffer, index, count);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Char], [])),
      function Write(ch) {
          var bytes = this.m_encoding.$encode(ch);
          this.$writeBytes(bytes);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Char])], [])),
      function Write(chars) {
          var charString = JSIL.StringFromCharArray(chars);
          var bytes = this.m_encoding.$encode(charString);
          this.$writeBytes(bytes);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
            $.Int32
      ], [])),
      function Write(chars, index, count) {
          var charString = JSIL.StringFromCharArray(chars, index, count);
          var bytes = this.m_encoding.$encode(charString);
          this.$writeBytes(bytes);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Double], [])),
      function Write(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Int16], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromInt16(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.UInt16], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromUInt16(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Int32], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromInt32(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.UInt32], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromUInt32(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromInt64(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.UInt64], [])),
      function Write(value) {
          this.$writeBytes($jsilcore.BytesFromUInt64(value));
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.Single], [])),
      function Write(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function Write(value) {
          var bytes = this.m_encoding.$encode(value);

          this.Write7BitEncodedInt(bytes.length);
          this.$writeBytes(bytes);
      }
    );

    $.Method({ Static: false, Public: false }, "Write7BitEncodedInt",
      (new JSIL.MethodSignature(null, [$.Int32], [])),
      function Write7BitEncodedInt(value) {
          var buf = new Array(1);

          while (value >= 128) {
              buf[0] = (value & 0xFF) | 128;
              this.$writeBytes(buf);

              value = value >> 7;
          }

          buf[0] = (value & 0xFF);
          this.$writeBytes(buf);
      }
    );

    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function () {
          if (this.m_stream)
              this.m_stream.Close();

          this.m_stream = null;
      }
    );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.IO.BinaryWriter", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    JSIL.MethodSignature.Void
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])
  );

  $.ExternalMethod({ Static: false, Public: false }, "Dispose",
    new JSIL.MethodSignature(null, [$.Boolean], [])
  );

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"));
});
//? }