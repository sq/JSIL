//? include("../Utils/JSIL.Make64BitInt.js");

JSIL.ImplementExternals("System.UInt64", function ($) {
    var mscorlib = JSIL.GetCorlib();

    var locals = JSIL.Make64BitInt($, function () {
        return mscorlib.System.UInt64;
    });
    var lazy = locals.lazy;
    var me = locals.me;
    var ctor = locals.ctor;
    var mktemp = locals.mktemp;

    var maxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var minValue = lazy(function () {
        return ctor(0, 0, 0);
    });

    var zero = lazy(function () {
        return ctor(0, 0, 0);
    });

    var one = lazy(function () {
        return ctor(1, 0, 0);
    });

    var tempRS = mktemp();
    var tempDiv = mktemp();

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function UInt64_op_Division(n, d, result) {
        var equality = me().op_Equality,
          greaterThanOrEqual = me().op_GreaterThanOrEqual,
          subtraction = me().op_Subtraction,
          leftShift = me().op_LeftShift;

        if (equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var q = result;
        if (q)
            q.a = q.b = q.c = 0;
        else
            q = ctor(0, 0, 0);

        var r = tempDiv();
        r.a = r.b = r.c = 0;

        for (var i = 63; i >= 0; i--) {
            r = leftShift(r, 1, r);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var lk = i < 24 ? "a" :
                      i < 48 ? "b" : "c";
            var s = (i - 24 * li);

            r.a |= (n[lk] & (1 << s)) >>> s;

            if (greaterThanOrEqual(r, d)) {
                r = subtraction(r, d, r);
                q[lk] |= 1 << s;
            }
        }

        return q;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function UInt64_op_Modulus(n, d, result) {
        var equality = me().op_Equality,
          greaterThanOrEqual = me().op_GreaterThanOrEqual,
          subtraction = me().op_Subtraction,
          leftShift = me().op_LeftShift;

        if (equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var r = result || ctor(0, 0, 0);
        r.a = r.b = r.c = 0;

        for (var i = 63; i >= 0; i--) {
            r = leftShift(r, 1, r);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var lk = i < 24 ? "a" :
                      i < 48 ? "b" : "c";
            var s = (i - 24 * li);

            r.a |= (n[lk] & (1 << s)) >>> s;

            if (greaterThanOrEqual(r, d)) {
                r = subtraction(r, d, r);
            }
        }

        return r;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
      (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
      function UInt64_op_RightShift(a, n, result) {

          n = n & 0x3f;

          if (n > 24) {
              return mscorlib.System.UInt64.op_RightShift(
                mscorlib.System.UInt64.op_RightShift(a, 24, tempRS()), n - 24, result
              );
          }

          var m = (1 << n) - 1;
          var cr = (a.c & m) << (24 - n);
          var ct = a.c >>> n;
          var br = (a.b & m) << (24 - n);
          var bt = a.b >>> n;
          var at = a.a >>> n;

          if (!result) {
              result = ctor(at | br, bt | cr, ct);
          } else {
              result.a = at | br;
              result.b = bt | cr;
              result.c = ct;
          }

          return result;
      });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function UInt64_op_LessThan(a, b) {
        var adiff = a.c - b.c;
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = a.b - b.b;
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return a.a < b.a;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function UInt64_op_LessThanOrEqual(a, b) {
        var adiff = a.c - b.c;
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = a.b - b.b;
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return a.a <= b.a;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function UInt64_op_GreaterThan(a, b) {
        var adiff = a.c - b.c;
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = a.b - b.b;
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return a.a > b.a;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function UInt64_op_GreaterThanOrEqual(a, b) {
        var adiff = a.c - b.c;
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = a.b - b.b;
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return a.a >= b.a;
    });

    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function UInt64_toString() {
        var a = this;
        var ten = me().FromNumber(10);

        var s = "";

        do {
            var r = me().op_Modulus(a, ten);
            s = r.a.toString() + s;
            a = me().op_Division(a, ten);
        } while (me().op_GreaterThan(a, minValue()));

        return s;
    });

    $.Method({ Static: false, Public: true }, "ToString",
      new JSIL.MethodSignature("System.String", []),
      function UInt64_ToString() {
          return this.toString();
      }
    );

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToHex",
    new JSIL.MethodSignature("System.String", []),
    function UInt64_ToHex() {
        var s = this.a.toString(16);

        if (this.b > 0 || this.c > 0) {
            if (s.length < 6)
                s = (new Array(6 - s.length + 1)).join('0') + s;

            s = this.b.toString(16) + s;

            if (this.c > 0) {
                if (s.length < 12)
                    s = (new Array(12 - s.length + 1)).join('0') + s;

                s = this.c.toString(16) + s;
            }
        }

        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToInt64",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int64"), []),
    function UInt64_ToInt64() {
        return new mscorlib.System.Int64(this.a, this.b, this.c);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "Clone",
    new JSIL.MethodSignature($.Type, []),
    function UInt64_Clone() {
        return ctor(this.a, this.b, this.c);
    });

    $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function UInt64_Equals(rhs) {
        return UInt64.op_Equality(this, rhs);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromInt32",
    (new JSIL.MethodSignature($.Type, [$.Int32], [])),
    function UInt64_FromInt32(n) {
        if (n < 0)
            JSIL.RuntimeError("cannot construct UInt64 from negative number");

        // only using 48 bits

        var n0 = Math.floor(n);
        return ctor(
            (n0 & 0xffffff),
            (n0 >>> 24) & 0xffffff,
            0);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [$.Double], [])),
    function UInt64_FromNumber(n) {
        var sum = n < 0 ? 0x100000000 : 0;
        return me().FromNumberImpl(sum + n, ctor);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToUInt32",
    (new JSIL.MethodSignature($.UInt32, [], [])),
    function UInt64_FromUInt32() {
        return ((0x1000000 * this.b) + this.a) >>> 0;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature($.Double, [], [])),
    function UInt64_ToNumber(maxValue, signed) {
        if (arguments.length === 0 || maxValue == -1)
            return 0x1000000 * (0x1000000 * this.c + this.b) + this.a;

        var truncated = me()
          .op_BitwiseAnd(this, me().FromNumber(maxValue))
          .ToNumber();

        if (signed) {
            var maxPlusOne = maxValue + 1;
            var signedMaxValue = maxValue >>> 1;
            if (truncated > signedMaxValue)
                return truncated - signedMaxValue;
            else
                return truncated;
        }
        else {
            return truncated;
        }
    });

    $.Method({ Static: false, Public: true }, "GetHashCode",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function UInt64_GetHashCode() {
      return this.a | ((this.b & 0xff) << 24);
    });
});

JSIL.MakeStruct("System.ValueType", "System.UInt64", true, [], function ($) {
    $.Field({ Static: false, Public: false }, "a", $.Int32);
    $.Field({ Static: false, Public: false }, "b", $.Int32);
    $.Field({ Static: false, Public: false }, "c", $.Int32);

    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );

    JSIL.MakeIConvertibleMethods($);
});