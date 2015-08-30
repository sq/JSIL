JSIL.ImplementExternals("System.IO.MemoryStream", function ($) {
    var ctorBytesImpl = function (self, bytes, index, count, writable) {
        System.IO.Stream.prototype._ctor.call(self);

        self._buffer = bytes;
        self._writable = writable;
        self._length = bytes.length;
        self._capacity = count;
        self._pos = index;
    };

    $.Method({ Static: false, Public: true }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
          ctorBytesImpl(this, [], 0, 0, true);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
      function _ctor(buffer) {
          ctorBytesImpl(this, buffer, 0, buffer.length, true);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Boolean], [])),
      function _ctor(buffer, writable) {
          ctorBytesImpl(this, buffer, 0, buffer.length, writable);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      /* Initializes a new non-resizable instance of the MemoryStream class based on the specified region of a 
         byte array, with the CanWrite property set as specified. */
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, $.Int32, $.Boolean], [])),
      function _ctor(buffer, index, count, writable) {
          ctorBytesImpl(this, buffer, index, count, writable);
      }
    );

    $.Method({ Static: false, Public: true }, "GetBuffer",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [], [])),
      function GetBuffer() {
          return this._buffer;
      }
    );

    $.Method({ Static: false, Public: true }, "ToArray",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [], [])),
      function ToArray() {
          return JSIL.Array.Clone(this._buffer);
      }
    );
});

JSIL.ImplementExternals(
  "System.IO.MemoryStream", $bytestream
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.IO.Stream"), "System.IO.MemoryStream", true, [], function ($) {
  $.Field({ Static: false, Public: false }, "_buffer", $jsilcore.TypeRef("System.Array", [$.Byte]));

  $.Field({ Static: false, Public: false }, "_capacity", $.Int32);

  $.Field({ Static: false, Public: false }, "_expandable", $.Boolean);

  $.Field({ Static: false, Public: false }, "_exposable", $.Boolean);

  $.Field({ Static: false, Public: false }, "_isOpen", $.Boolean);

  $.Field({ Static: false, Public: false }, "_length", $.Int32);

  $.Field({ Static: false, Public: false }, "_origin", $.Int32);

  $.Field({ Static: false, Public: false }, "_position", $.Int32);

  $.Field({ Static: false, Public: false }, "_writable", $.Boolean);

  $.Constant({ Static: true, Public: false }, "MemStreamMaxLength", 2147483647);

  $.Property({ Static: false, Public: true, Virtual: true }, "CanRead");

  $.Property({ Static: false, Public: true, Virtual: true }, "CanSeek");

  $.Property({ Static: false, Public: true, Virtual: true }, "CanWrite");

  $.Property({ Static: false, Public: true, Virtual: true }, "Capacity");

  $.Property({ Static: false, Public: true, Virtual: true }, "Length");

  $.Property({ Static: false, Public: true, Virtual: true }, "Position");
});
//? }