/*? if (!('JSIL_Make64BitInt' in __out)) { __out.JSIL_Make64BitInt = true; */
//  Since alot of operators are shared between Int64 and UInt64, we construct both types using this function
JSIL.Make64BitInt = function ($, _me) {
  var mscorlib = JSIL.GetCorlib();

  function lazy(f) {
    var state = null;
    return function getLazyValue() {
      if (state === null)
        state = f();
      return state;
    };
  };

  var me = lazy(_me);

  var ctor = function ctor(a, b, c) {
    return new (me())(a, b, c);
  };

  var mktemp = function () {
    return lazy(function () {
      return ctor(0, 0, 0);
    });
  };

  var maxValue = lazy(function () {
    return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
  });

  var zero = lazy(function () {
    return ctor(0, 0, 0);
  });

  var one = lazy(function () {
    return ctor(1, 0, 0);
  });

  var tempLS = mktemp();
  var tempMul1 = mktemp();
  var tempMul2 = mktemp();

  var makeOrReturn = function makeOrReturn(result, a, b, c) {
    if (result) {
      result.a = a & 0xffffff;
      result.b = b & 0xffffff;
      result.c = c & 0xffff;
      return result;
    }

    return ctor(a & 0xffffff, b & 0xffffff, c & 0xffff);
  };

  var tryParse =
    function xInt64_TryParse(text, style, result) {
      var r = zero();

      var radix = 10;

      if (style & System.Globalization.NumberStyles.AllowHexSpecifier)
        radix = 16;

      var rdx = ctor(radix, 0, 0);
      var neg = false;

      for (var i = 0; i < text.length; i++) {
        if (i == 0 && text[i] == '-') {
          neg = true;
          continue;
        }
        var c = parseInt(text[i], radix);
        if (isNaN(c)) {
          result.set(zero());
          return false;
        }
        r = me().op_Addition(ctor(c, 0, 0), me().op_Multiplication(rdx, r));
      }

      if (neg)
        r = me().op_UnaryNegation(r);

      result.set(r);

      return true;
    };

  $.RawMethod(true, "Create", function xInt64_Create(a, b, c) {
    return new (me())(a, b, c);
  });

  $.RawMethod(true, "FromBytes", function xInt64_FromBytes(bytes, offset) {
    var a = (bytes[offset + 0] << 0) |
      (bytes[offset + 1] << 8) |
      (bytes[offset + 2] << 16);
    var b = (bytes[offset + 3] << 0) |
      (bytes[offset + 4] << 8) |
      (bytes[offset + 5] << 16);
    var c = (bytes[offset + 6] << 0) |
      (bytes[offset + 7] << 8);
    return new (me())(a, b, c);
  });

  $.RawMethod(false, ".ctor", function xInt64__ctor(a, b, c) {
    this.a = a | 0;
    this.b = b | 0;
    this.c = c | 0;
  });

  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature($.Type, ["System.String"], [])),
    function xInt64_Parse(text) {
      var result = new JSIL.BoxedVariable(null);
      if (!tryParse(text, 0, result))
        throw new System.Exception("NumberParseException");

      return result.get();
    });

  $.Method({ Static: true, Public: true }, "op_Addition",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_Addition(a, b, result) {
          var ca = a.a + b.a;
          var ra = (ca & 0xffffff000000) >> 24;
          var cb = ra + a.b + b.b;
          var rb = (cb & 0xffffff000000) >> 24;
          var cc = rb + a.c + b.c;

          return makeOrReturn(result, ca, cb, cc);
        });

  $.Method({ Static: true, Public: true }, "op_Subtraction",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_Subtraction(a, b, result) {
          var ca = (a.a - b.a) | 0;
          var ra = 0;
          if (ca < 0) {
            ca = 0x1000000 + ca;
            ra = -1;
          }
          var cb = (ra + ((a.b - b.b) | 0)) | 0;
          var rb = 0;
          if (cb < 0) {
            cb = 0x1000000 + cb;
            rb = -1;
          }
          var cc = (rb + ((a.c - b.c) | 0)) | 0;
          if (cc < 0) {
            cc = 0x10000 + cc;
          }

          return makeOrReturn(result, ca, cb, cc);
        });

  $.Method({ Static: true, Public: true }, "op_LeftShift",
        (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
         function xInt64_op_LeftShift(a, n, result) { // a is UInt64, n is Int32
           if (!result)
             result = ctor(0, 0, 0);

           n = n & 0x3f;

           var maxShift = 8;
           if (n > 8) {
             return me().op_LeftShift(me().op_LeftShift(a, maxShift, tempLS()), n - maxShift, result);
           }

           var bat = a.a << n;
           var ba = bat & 0xffffff;
           var ra = (bat >>> 24) & 0xffffff;
           var bbt = (a.b << n) | ra;
           var bb = bbt & 0xffffff;
           var rb = (bbt >>> 24) & 0xffff;
           var bct = a.c << n;
           var bc = (bct & 0xffff) | rb;

           return makeOrReturn(result, ba, bb, bc);
         });

  $.Method({ Static: true, Public: true }, "op_OnesComplement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function xInt64_op_OnesComplement(a, result) {
          return makeOrReturn(result, ~a.a, ~a.b, ~a.c);
        });

  $.Method({ Static: true, Public: true }, "op_ExclusiveOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_ExclusiveOr(a, b, result) {
          return makeOrReturn(result, a.a ^ b.a, a.b ^ b.b, a.c ^ b.c);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseAnd",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_BitwiseAnd(a, b, result) {
          return makeOrReturn(result, a.a & b.a, a.b & b.b, a.c & b.c);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_BitwiseOr(a, b, result) {
          return makeOrReturn(result, a.a | b.a, a.b | b.b, a.c | b.c);
        });


  $.Method({ Static: true, Public: true }, "op_Equality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function xInt64_op_Equality(a, b) {
          return a.a === b.a && a.b === b.b && a.c === b.c;
        });

  $.Method({ Static: true, Public: true }, "op_Inequality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function xInt64_op_Inequality(a, b) {
          return a.a !== b.a || a.b !== b.b || a.c !== b.c;
        });

  $.Method({ Static: true, Public: true }, "op_Decrement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function xInt64_op_Decrement(a, result) {
          if (a.a > 0)
            return makeOrReturn(result, a.a - 1, a.b, a.c);
          else
            return me().op_Subtraction(a, one(), result);
        });

  $.Method({ Static: true, Public: true }, "op_Increment",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function xInt64_op_Increment(a, result) {
          if (a.a < 0xffffff)
            return makeOrReturn(result, a.a + 1, a.b, a.c);
          else
            return me().op_Addition(a, one(), result);
        });

  $.Method({ Static: true, Public: true }, "op_Multiplication",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function xInt64_op_Multiplication(a, b, result) {
          if (me().op_Equality(a, zero()) || me().op_Equality(b, zero()))
            return zero();

          if (mscorlib.System.UInt64.op_GreaterThan(a, b))
            return me().op_Multiplication(b, a, result);

          var s = result;
          if (!s)
            s = ctor(0, 0, 0);

          if (a.a & 1 == 1) {
            s.a = b.a;
            s.b = b.b;
            s.c = b.c;
          }

          var l = one();

          while (!me().op_Equality(a, l)) {
            a = mscorlib.System.UInt64.op_RightShift(a, 1, tempMul1());
            b = me().op_LeftShift(b, 1, tempMul2());

            if (a.a & 1 == 1)
              s = me().op_Addition(b, s, s);
          }

          return s;
        });

  $.RawMethod(true, "CheckType", function xInt64_CheckType(value) {
    return value &&
      (typeof value.a === "number") &&
      (typeof value.b === "number") &&
      (typeof value.c === "number");
  });

  $.RawMethod(false, "valueOf", function xInt64_valueOf() {
    return this.ToNumber();
  });

  $.RawMethod(true, "FromNumberImpl", function (n, makeResult) {
    if (n < 0)
      JSIL.RuntimeError("cannot construct UInt64 from negative number");

    var bits24 = 0xffffff;

    var floorN = Math.floor(n);
    var n0 = floorN | 0;
    var n1 = (floorN / 0x1000000) | 0;
    var n2 = (floorN / 0x1000000000000) | 0;

    return makeResult(
        (n0 & bits24),
        (n1 & bits24),
        (n2 & bits24)
    );
  });

  var $formatSignature = function () {
    return ($formatSignature = JSIL.Memoize(new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [
        $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef($.Type.__FullName__),
        $jsilcore.TypeRef("System.IFormatProvider")
    ])))();
  };

  $.RawMethod(
    true, "$ToString",
    function $ToString(self, format, formatProvider) {
      return $formatSignature().CallStatic($jsilcore.JSIL.System.NumberFormatter, "NumberToString", null, format, self, formatProvider).toString();
    }
  );

  return {
    lazy: lazy,
    me: me,
    ctor: ctor,
    mktemp: mktemp
  };
};
/*? }*/