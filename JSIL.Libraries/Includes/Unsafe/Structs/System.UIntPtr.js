JSIL.ImplementExternals("System.UIntPtr", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.UInt32], [])),
      function _ctor(value) {
          this.value = value >>> 0;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.UInt64], [])),
      function _ctor(value) {
          this.value = value.ToNumber() >>> 0;
      }
    );

    $.Method({ Static: false, Public: true }, "ToUInt32",
      (new JSIL.MethodSignature($.UInt32, [], [])),
      function ToUInt32() {
          return this.value;
      }
    );

    $.Method({ Static: false, Public: true }, "ToUInt64",
      (new JSIL.MethodSignature($.UInt64, [], [])),
      function ToUInt64() {
          return $jsilcore.System.UInt64.FromUInt32(this.value);
      }
    );
});



JSIL.MakeStruct("System.ValueType", "System.UIntPtr", true, [], function ($) {
    $.Field({ Static: false, Public: false }, "value", $.UInt32);

    $.Field({ Static: true, Public: true }, "Zero", $.Type);
});