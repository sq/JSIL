//? include("../Utils/JSIL.Make64BitInt.js");

JSIL.ImplementExternals("System.Int64", function ($) {

    // The unsigned range 0 to 0x7FFFFFFFFFFFFFFF (= Int64.MaxValue) is positive: 0 to 9223372036854775807
    // The directly following unsigned range 0x8000000000000000 (= Int64.MaxValue + 1 = Int64.MinValue) to 0xFFFFFFFFFFFFFFFF is negative: -9223372036854775808 to -1
    //
    //  signed value
    //  ^
    //  |      /
    //  |    /
    //  |  /
    //  |/z
    //  ------------------> unsigned value
    //  |              /
    //  |            /
    //  |          /
    //  |        /
    //

    var mscorlib = JSIL.GetCorlib();

    var locals = JSIL.Make64BitInt($, function () {
        return mscorlib.System.Int64;
    });
    var lazy = locals.lazy;
    var me = locals.me;
    var ctor = locals.ctor;
    var mktemp = locals.mktemp;

    var zero = lazy(function () {
        return ctor(0, 0, 0);
    });

    var one = lazy(function () {
        return ctor(1, 0, 0);
    });

    var minusOne = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var signedMaxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0x7FFF);
    });

    var unsignedMaxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var tempRS = mktemp();
    var tempRS1 = mktemp();
    var tempRS2 = mktemp();
    var tempRS3 = mktemp();

    var tempTS = mktemp();
    var tempDiv = mktemp();
    var tempDiv2 = mktemp();
    var tempDiv3 = mktemp();
    var tempDiv4 = mktemp();
    var tempMod = mktemp();
    var tempMod2 = mktemp();
    var tempMod3 = mktemp();
    var tempMod4 = mktemp();
    var tempN = mktemp();

    var isNegative = function Int64_IsNegative(a) {
        return mscorlib.System.UInt64.op_GreaterThan(a, signedMaxValue());
    };

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_UnaryNegation",
    (new JSIL.MethodSignature($.Type, [$.Type], [])),
    function Int64_op_UnaryNegation(a, result) {
        var complement = me().op_Subtraction(unsignedMaxValue(), a, tempN());
        return me().op_Addition(complement, one(), result);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function Int64_op_Division(n, d, result) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (!result)
            result = ctor(0, 0, 0);
        else
            result.a = result.b = result.c = 0;

        if (isNegative(d))
            return me().op_Division(
              me().op_UnaryNegation(n, tempDiv()), me().op_UnaryNegation(d, tempDiv2()), result
            );
        else if (isNegative(n))
            return me().op_UnaryNegation(
              me().op_Division(
                me().op_UnaryNegation(n, tempDiv3()), d, tempDiv4()
              ), result
            );
        else
            return mscorlib.System.UInt64.op_Division(n, d, result);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function Int64_op_Modulus(n, d, result) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (!result)
            result = ctor(0, 0, 0);

        if (isNegative(d))
            return me().op_Modulus(
              me().op_UnaryNegation(n, tempMod()), me().op_UnaryNegation(d, tempMod2()), result
            );
        else if (isNegative(n))
            return me().op_UnaryNegation(
              me().op_Modulus(
                me().op_UnaryNegation(n, tempMod3()), d, tempMod4()
              ), result
            );
        else
            // fix return type error
            return mscorlib.System.UInt64.op_Modulus(n, d, result);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
    (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
    function Int64_op_RightShift(a, n, result) {
        // Int64 (signed) uses arithmetic shift, UIn64 (unsigned) uses logical shift
        if (!result)
            result = ctor(0, 0, 0);

        if (n === 0) {
            var result2 = a;
        } else if (n > 32) {
            result2 = me().op_RightShift(me().op_RightShift(a, 32), n - 32);
        } else {
            var unsignedShift = mscorlib.System.UInt64.op_RightShift(a, n);

            if (isNegative(a)) {
                var outshift = mscorlib.System.UInt64.op_RightShift(mscorlib.System.UInt64.Create(16777215, 16777215, 65535), n);
                var inshift = mscorlib.System.UInt64.op_LeftShift(outshift, 64 - n);
                var uresult = mscorlib.System.UInt64.op_BitwiseOr(unsignedShift, inshift);
            } else {
                uresult = unsignedShift;
            }
            result2 = (uresult).ToInt64();
        }
        return result2;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function Int64_op_GreaterThan(a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_GreaterThan(a, b);
        else
            return bn;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function Int64_op_GreaterThanOrEqual(a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_GreaterThanOrEqual(a, b);
        else
            return bn;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function Int64_op_LessThan(a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_LessThan(a, b);
        else
            return an;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function Int64_op_LessThanOrEqual(a, b) {
        var an = isNegative(a);
        var bn = isNegative(b);

        if (an === bn)
            return mscorlib.System.UInt64.op_LessThanOrEqual(a, b);
        else
            return an;
    });

    // Might need to be implemented externally
    $.Method({ Static: false, Public: true }, "Equals",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), []),
    function Int64_Equals(a) {
        return me().op_Equality(this, a);
    });

    // Might need to be implemented externally
    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function Int64_toString() {
        var s = "";
        var a = this;
        if (isNegative(this)) {
            s += "-";
            a = me().op_UnaryNegation(this, tempTS());
        }
        s += mscorlib.System.UInt64.prototype.toString.apply(a);
        return s;
    });

    $.Method({ Static: false, Public: true }, "ToString",
      new JSIL.MethodSignature("System.String", []),
      function Int64_ToString() {
          return this.toString();
      }
    );

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromInt32",
    (new JSIL.MethodSignature($.Type, [$.Int32], [])),
    function Int64_FromInt32(n) {
        var sign = n < 0 ? -1 : 1;
        n = Math.abs(n);

        var n0 = Math.floor(n);
        var r = ctor(n0 & 0xffffff, (n0 >>> 24) & 0xffffff, 0);

        if (sign == -1)
            return me().op_UnaryNegation(r);
        else
            return r;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [$.Double], [])),
    function Int64_FromNumber(n) {
        var sign = n < 0 ? -1 : 1;
        var r = me().FromNumberImpl(Math.abs(n), ctor);

        if (sign == -1)
            return me().op_UnaryNegation(r, r);
        else
            return r;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToUInt64",
    new JSIL.MethodSignature($.UInt64, []),
    function Int64_ToUInt64() {
        return new mscorlib.System.UInt64(this.a, this.b, this.c);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToInt32",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function Int64_ToInt32() {
        var neg = isNegative(this);
        var n = neg ? me().op_UnaryNegation(this) : this;
        var r = (0x1000000 * (n.b) + n.a) >>> 0;

        if (neg)
            return -r;
        else
            return r;
    });

    $.Method({ Static: false, Public: true }, "GetHashCode",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function Int64_GetHashCode() {
      return this.a | ((this.b & 0xff) << 24);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature($.Double, [], [])),
    function Int64_ToNumber(maxValue, signed) {
        if (arguments.length === 0 || maxValue == -1) {
            var neg = isNegative(this);
            var n = neg ? me().op_UnaryNegation(this) : this;
            var r = 0x1000000 * (0x1000000 * n.c + n.b) + n.a;

            if (neg)
                return -r;
            else
                return r;
        }

        var truncated = me()
          .op_BitwiseAnd(this, me().FromNumber(maxValue))
          .ToNumber();

        if (signed) {
            var signedMaxValue = maxValue >>> 1;
            if (truncated > signedMaxValue)
                return (truncated - maxValue) - 1;
            else
                return truncated;
        }
        else {
            return truncated;
        }
    });
});

JSIL.MakeStruct("System.ValueType", "System.Int64", true, [], function ($) {
    $.Field({ Static: false, Public: false }, "a", $.Int32);
    $.Field({ Static: false, Public: false }, "b", $.Int32);
    $.Field({ Static: false, Public: false }, "c", $.Int32);

    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );

    JSIL.MakeIConvertibleMethods($);
});