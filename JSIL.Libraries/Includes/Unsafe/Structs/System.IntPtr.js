JSIL.ImplementExternals("System.IntPtr", function ($) {
    var tIntPtr = $.IntPtr;

    $.RawMethod(false, "$fromPointer", function (pointer) {
        this.pointer = pointer;
        this.value = null;
    });

    $.RawMethod(false, "$fromInt32", function (int32) {
        this.pointer = null;
        this.value = int32 | 0;
    });

    $.RawMethod(false, "$fromInt64", function (int64) {
        this.pointer = null;
        this.value = int64.ToNumber() | 0;
    });

    $.RawMethod(true, ".cctor", function () {
        System.IntPtr.Zero = new System.IntPtr();
    });

    $.Method({ Static: false, Public: false }, ".ctor",
      (new JSIL.MethodSignature(null, [], [])),
      function _ctor() {
          this.pointer = null;
          this.value = 0;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Int32], [])),
      function _ctor(value) {
          this.pointer = null;
          this.value = value | 0;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function _ctor(value) {
          this.pointer = null;
          this.value = value.ToNumber() | 0;
      }
    );

    $.RawMethod(false, "__CopyMembers__",
      function IntPtr_CopyMembers(source, target) {
          target.value = source.value;
          if (!source.pointer)
              target.pointer = null;
          else
              target.pointer = source.pointer;
      }
    );

    function isNullPointer(p) {
        return (!p.pointer && p.value === 0) ||
               (p.pointer && p.pointer.offsetInBytes === 0);
    };

    $.Method({ Static: true, Public: true }, "op_Equality",
      (new JSIL.MethodSignature($.Boolean, [tIntPtr, tIntPtr], [])),
      function op_Equality(lhs, rhs) {
          // Null pointers always equal, regardless of where they came from      
          if (isNullPointer(lhs) && isNullPointer(rhs)) {
              return true;
          }

          if (lhs.pointer) {
              if (!rhs.pointer) // Non-null emscripten pointers can't equal C# ones
                  return false;

              return rhs.pointer.equals(lhs.pointer);
          } else {
              return lhs.value === rhs.value;
          }
      }
    );

    $.Method({ Static: true, Public: true }, "op_Inequality",
      (new JSIL.MethodSignature($.Boolean, [tIntPtr, tIntPtr], [])),
      function op_Inequality(lhs, rhs) {
          if (isNullPointer(lhs) && isNullPointer(rhs)) {
              return false;
          }

          if (lhs.pointer) {
              if (!rhs.pointer)
                  return true;

              return !rhs.pointer.equals(lhs.pointer);
          } else {
              return lhs.value !== rhs.value;
          }
      }
    );

    $.Method({ Static: true, Public: true }, "op_Addition",
      (new JSIL.MethodSignature(tIntPtr, [tIntPtr, $.Int32], [])),
      function op_Addition(lhs, rhs) {
          if (lhs.pointer) {
              var newPointer = lhs.pointer.add(rhs, false);

              return JSIL.CreateInstanceOfType(
                System.IntPtr.__Type__,
                "$fromPointer",
                [newPointer]
              );
          } else {
              return JSIL.CreateInstanceOfType(
                System.IntPtr.__Type__,
                "$fromInt32",
                [lhs.value + rhs]
              );
          }
      }
    );

    $.Method({ Static: false, Public: true }, "ToInt32",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function ToInt32() {
          if (this.pointer) {
              return this.pointer.offsetInBytes;
          } else {
              return this.value;
          }
      }
    );

    $.Method({ Static: false, Public: true }, "ToInt64",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function ToInt64() {
          if (this.pointer) {
              return $jsilcore.System.Int64.FromInt32(
                this.pointer.offsetInBytes
              );
          } else {
              return $jsilcore.System.Int64.FromInt32(
                this.value
              );
          }
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$.Int32]),
      function op_Explicit(value) {
          return JSIL.CreateInstanceOfType(
            System.IntPtr.__Type__,
            "$fromInt32",
            [value]
          );
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$.Int64]),
      function op_Explicit(value) {
          return JSIL.CreateInstanceOfType(
            System.IntPtr.__Type__,
            "$fromInt64",
            [value]
          );
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$jsilcore.TypeRef("JSIL.Pointer", [$jsilcore.TypeRef("System.Void")])]),
      function op_Explicit(value) {
          return JSIL.CreateInstanceOfType(
            System.IntPtr.__Type__,
            "$fromPointer",
            [value]
          );
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.Pointer", [$jsilcore.TypeRef("System.Void")]), [$jsilcore.TypeRef("System.IntPtr")]),
      function op_Explicit(value) {
          if (value.pointer) {
              // FIXME: Cast it?
              return value.pointer;
          } else {
              throw new Error('Not implemented');
          }
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IntPtr")]),
      function op_Explicit(value) {
          if (value.pointer) {
              return value.pointer.offsetInBytes;
          } else {
              return value.value;
          }
      }
    );

    $.Method({ Static: true, Public: true }, "op_Explicit",
      new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.IntPtr")]),
      function op_Explicit(value) {
          if (value.pointer) {
              return $jsilcore.System.Int64.FromInt32(value.pointer.offsetInBytes);
          } else {
              return $jsilcore.System.Int64.FromInt32(value.value);
          }
      }
    );
});

JSIL.MakeStruct("System.ValueType", "System.IntPtr", true, [], function ($) {
    $.Field({ Static: false, Public: false }, "value", $.Int32);

    $.Field({ Static: true, Public: true }, "Zero", $.Type);
});