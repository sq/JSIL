JSIL.ImplementExternals("System.IO.Stream", function ($) {
    var readByteBuffer = null;

    $.Method({ Static: false, Public: true }, "ReadByte",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function ReadByte() {
          if (!readByteBuffer)
              readByteBuffer = JSIL.Array.New(System.Byte, 1);

          var count = this.Read(readByteBuffer, 0, 1);

          if (count >= 1)
              return readByteBuffer[0];
          else
              return -1;
      }
    );

    $.Method({ Static: false, Public: true }, "Close",
      (JSIL.MethodSignature.Void),
      function Close() {
          if (this._onClose) {
              this._onClose();
              this._onClose = null;
          }
      }
    );

    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function Dispose() {
          if (this._onClose) {
              this._onClose();
              this._onClose = null;
          }
      }
    );

    $.Method({ Static: false, Public: true }, "CopyTo",
      (new JSIL.MethodSignature(null, [$.Type], [])),
      function CopyTo(stream) {
          if (this._buffer) {
              stream.Write(this._buffer, 0, this._length);
          } else {
              throw new Error("Copying not implemented for this stream type");
          }
      }
    );

    $.Method({ Static: false, Public: true }, "CopyToAsync",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.IO.Stream")], [])),
      function CopyToAsync(stream) {
          var tcs = new (System.Threading.Tasks.TaskCompletionSource$b1.Of(System.Boolean))();
          if (this._buffer) {
              stream.Write(this._buffer, 0, this._length);
          } else {
              throw new Error("Copying not implemented for this stream type");
          }
          tcs.TrySetResult(true);
          return tcs.Task;
      }
    );

    $.RawMethod(false, "$GetURI", function () {
        return null;
    });
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.MarshalByRefObject"), "System.IO.Stream", true, [], function ($) {
  $.Constant({ Static: true, Public: false }, "_DefaultBufferSize", 4096);

  $.Field({ Static: true, Public: true }, "Null", $.Type);

  $.Property({ Static: false, Public: true, Virtual: true }, "CanRead");

  $.Property({ Static: false, Public: true, Virtual: true }, "CanSeek");

  $.Property({ Static: false, Public: true, Virtual: true }, "CanTimeout");

  $.Property({ Static: false, Public: true, Virtual: true }, "CanWrite");

  $.Property({ Static: false, Public: true, Virtual: true }, "Length");

  $.Property({ Static: false, Public: true, Virtual: true }, "Position");

  $.Property({ Static: false, Public: true, Virtual: true }, "ReadTimeout");

  $.Property({ Static: false, Public: true, Virtual: true }, "WriteTimeout");

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
});
//? }