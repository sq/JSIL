JSIL.ImplementExternals("System.IO.StreamReader", function ($) {
    var UTF8 = function () { return $jsilcore.System.Text.Encoding.get_UTF8(); };
    var UTF16LE = function () { return $jsilcore.System.Text.Encoding.get_Unicode(); };
    var UTF16BE = function () { return $jsilcore.System.Text.Encoding.get_BigEndianUnicode(); };

    $.RawMethod(false, "$ctorImpl",
      function _ctorImpl(stream, encoding, detectEncoding) {
          this.stream = stream;
          this.encoding = encoding;

          if (detectEncoding) {
              var originalPosition = this.stream.get_Position();
              var buf = JSIL.Array.New(System.Byte, 4);
              var bytesRead = this.stream.Read(buf, 0, buf.length);
              var bytesToSkip = 0;

              if (
                (bytesRead >= 3) &&
                (buf[0] === 239) &&
                (buf[1] === 187) &&
                (buf[2] === 191)
              ) {
                  bytesToSkip = 3;
                  this.encoding = UTF8();
              } else if (
                (bytesRead >= 2)
              ) {
                  if (
                    (buf[0] === 255) &&
                    (buf[1] === 254)
                  ) {
                      bytesToSkip = 2;
                      this.encoding = UTF16LE();
                  } else if (
                    (buf[0] === 254) &&
                    (buf[1] === 255)
                  ) {
                      bytesToSkip = 2;
                      this.encoding = UTF16BE();
                  }
              }


              var tInt64 = $jsilcore.System.Int64;
              var resultPosition = tInt64.op_Addition(originalPosition, tInt64.FromInt32(bytesToSkip));
              this.stream.set_Position(resultPosition);
          }
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function _ctor(path) {
          this.$ctorImpl(
            new System.IO.FileStream(path, System.IO.FileMode.Open),
            UTF8(), true
          );
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])),
      function _ctor(stream) {
          this.$ctorImpl(stream, UTF8(), true);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $.Boolean], [])),
      function _ctor(path, detectEncoding) {
          this.$ctorImpl(
            new System.IO.FileStream(path, System.IO.FileMode.Open),
            UTF8(), detectEncoding
          );
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $.Boolean], [])),
      function _ctor(stream, detectEncoding) {
          this.$ctorImpl(stream, UTF8(), detectEncoding);
      }
    );

    $.Method({ Static: false, Public: false }, "Dispose",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function Dispose(disposing) {
          this.stream = null;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadLine",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadLine() {
          var line = "";

          while (true) {
              var ch = $jsilio.ReadCharFromStream(this.stream, this.encoding);
              if (!ch) {
                  if (line.length === 0)
                      return null;
                  else
                      break;
              } else if (ch === "\r") {
                  var next = this.stream.$PeekByte();
                  if (next === 10)
                      continue;
                  else if (next === -1) {
                      if (line.length === 0)
                          return null;
                      else
                          break;
                  } else
                      break;
              } else if (ch === "\n") {
                  break;
              }

              line += ch;
          };

          return line;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadToEnd",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadToEnd() {
          var result = "";

          while (true) {
              var ch = $jsilio.ReadCharFromStream(this.stream, this.encoding);
              if (!ch)
                  break;

              result += ch;
          }

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "get_EndOfStream",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_EndOfStream() {
          var tInt64 = $jsilcore.System.Int64;
          return tInt64.op_Equality(this.stream.get_Position(), this.stream.get_Length());
      }
    );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.IO.TextReader"), "System.IO.StreamReader", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $.Boolean], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding"),
        $.Boolean
    ], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding"),
        $.Boolean, $.Int32
    ], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.String], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.String, $.Boolean], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Text.Encoding")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [
        $.String, $jsilcore.TypeRef("System.Text.Encoding"),
        $.Boolean
    ], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [
        $.String, $jsilcore.TypeRef("System.Text.Encoding"),
        $.Boolean, $.Int32
    ], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "Close",
    JSIL.MethodSignature.Void
  );

  $.ExternalMethod({ Static: false, Public: false }, "Dispose",
    new JSIL.MethodSignature(null, [$.Boolean], [])
  );

  $.Property({ Static: false, Public: true }, "EndOfStream");
});
//? }