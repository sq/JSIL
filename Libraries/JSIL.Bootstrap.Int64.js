"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

//
//  Since alot of operators are shared between Int64 and UInt64, we construct both types using this function
JSIL.Make64BitInt = function ($, ctor, me) {

  var mscorlib = JSIL.GetCorlib();

  function lazy(f) {
      var state = null;
      return function () {
          if (state === null)
              state = f();
          return state;
      };
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

  var tryParse =
    function (text, style, result) {
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
          result.value = zero();
          return false;
        }
        r = me().op_Addition(ctor(c, 0, 0), me().op_Multiplication(rdx, r));
      }

      if (neg)
        r = me().op_UnaryNegation(r);

      result.value = r;

      return true;
    };

  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature($.Type, ["System.String"], [])),
    function (text) {
      var result = { value: null };
      if (!tryParse(text, 0, result))
        throw new System.Exception("NumberParseException");

      return result.value;
    });

  $.RawMethod({ Static: true, Public: true }, "Create",
        //(new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32")], [])),
        ctor);

  $.Method({ Static: true, Public: true }, "op_Addition",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          var ca = ad[0] + bd[0];
          var ra = (ca & 0xffffff000000) >> 24;
          var cb = ra + ad[1] + bd[1];
          var rb = (cb & 0xffffff000000) >> 24;
          var cc = rb + ad[2] + bd[2];
          return ctor(ca & 0xffffff, cb & 0xffffff, cc & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_Subtraction",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          var ca = ad[0] - bd[0];
          var ra = 0;
          if (ca < 0) {
            ca = 0x1000000 + ca;
            ra = -1;
          }
          var cb = ra + ad[1] - bd[1];
          var rb = 0;
          if (cb < 0) {
            cb = 0x1000000 + cb;
            rb = -1;
          }
          var cc = rb + ad[2] - bd[2];
          if (cc < 0) {
            cc = 0x10000 + cc;
          }
          return ctor(ca & 0xffffff, cb & 0xffffff, cc & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_LeftShift",
        (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
         function (a, n) { // a is UInt64, n is Int32
           if (n < 0) {
             return me().op_RightShift(a, -n);
           }

           if (n > 24) {
             return me().op_LeftShift(me().op_LeftShift(a, 24), n - 24);
           }

           var ad = a.data;
           var bat = ad[0] << n;
           var ba = bat & 0xffffff;
           var ra = (bat >>> 24) & 0xffffff;
           var bbt = (ad[1] << n) | ra;
           var bb = bbt & 0xffffff;
           var rb = (bbt >>> 24) & 0xffff;
           var bct = ad[2] << n;
           var bc = (bct & 0xffff) | rb;
           return ctor(ba, bb, bc);
         });

  $.Method({ Static: true, Public: true }, "op_OnesComplement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          return ctor((~ad[0]) & 0xffffff, (~ad[1]) & 0xffffff, (~ad[2]) & 0xffff);
        });

  $.Method({ Static: true, Public: true }, "op_ExclusiveOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] ^ bd[0], ad[1] ^ bd[1], ad[2] ^ bd[2]);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseAnd",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] & bd[0], ad[1] & bd[1], ad[2] & bd[2]);
        });

  $.Method({ Static: true, Public: true }, "op_BitwiseOr",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ctor(ad[0] | bd[0], ad[1] | bd[1], ad[2] | bd[2]);
        });


  $.Method({ Static: true, Public: true }, "op_Equality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ad[0] === bd[0] && ad[1] === bd[1] && ad[2] === bd[2];
        });

  $.Method({ Static: true, Public: true }, "op_Inequality",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
        function (a, b) {
          var ad = a.data;
          var bd = b.data;
          return ad[0] !== bd[0] || ad[1] !== bd[1] || ad[2] !== bd[2];
        });

  $.Method({ Static: true, Public: true }, "op_Decrement",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          if (ad[0] > 0)
            return ctor(ad[0] - 1, ad[1], ad[2]);
          else
            return me().op_Subtraction(a, one());
        });

  $.Method({ Static: true, Public: true }, "op_Increment",
        (new JSIL.MethodSignature($.Type, [$.Type], [])),
        function (a) {
          var ad = a.data;
          if (ad[0] < 0xffffff)
            return ctor(ad[0] + 1, ad[1], ad[2]);
          else
            return me().op_Addition(a, one());
        });

  $.Method({ Static: true, Public: true }, "op_Multiplication",
        (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
        function (a, b) {
          var s = zero();

          if (me().op_Equality(a, s) || me().op_Equality(b, s))
            return s;

          if (mscorlib.System.UInt64.op_GreaterThan(a, b))
            return me().op_Multiplication(b, a);


          if (a.data[0] & 1 == 1)
            s = b;

          var l = one();

          while (!me().op_Equality(a, l)) {
            a = mscorlib.System.UInt64.op_RightShift(a, 1);
            b = me().op_LeftShift(b, 1);

            if (a.data[0] & 1 == 1)
              s = me().op_Addition(b, s);
          }

          return s;
        });

  $.RawMethod(true, "CheckType", function (value) {
    return (typeof value.data === "object" && value.data.length == 3);
  });

};

JSIL.ImplementExternals("System.UInt64", function ($) {
    var ctor =
        function (a, b, c) {
            var r = new mscorlib.System.UInt64();
            r.data = [a, b, c];
            return r;
        };

    var mscorlib = JSIL.GetCorlib();

    var me = lazy(function () {
        return mscorlib.System.UInt64;
    });

    JSIL.Make64BitInt($, ctor, me);

    function lazy(f) {
        var state = null;
        return function () {
            if (state === null)
                state = f();
            return state;
        };
    };

    var maxValue = lazy(function () {
        return ctor(0xFFFFFF, 0xFFFFFF, 0xFFFF);
    });

    var minValue = lazy(function () {
        return ctor(0, 0, 0);
    });

    var one = lazy(function () {
        return ctor(1, 0, 0);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var q = ctor(0, 0, 0); // do not use UInt64.MinValue, we are mutating q !!
        var r = ctor(0, 0, 0);
        var nd = n.data;

        for (var i = 63; i >= 0; i--) {
            r = me().op_LeftShift(r, 1);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var s = (i - 24 * li);

            r.data[0] |= (nd[li] & (1 << s)) >>> s;

            if (me().op_GreaterThanOrEqual(r, d)) {
                r = me().op_Subtraction(r, d);
                q.data[li] |= 1 << s;
            }
        }

        return q;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, minValue()))
            throw new Error("System.DivideByZeroException");

        var r = ctor(0, 0, 0);
        var nd = n.data;

        for (var i = 63; i >= 0; i--) {
            r = me().op_LeftShift(r, 1);

            var li = i < 24 ? 0 :
                      i < 48 ? 1 : 2;
            var s = (i - 24 * li);

            r.data[0] |= (nd[li] & (1 << s)) >>> s;

            if (me().op_Equality(r, d) || me().op_GreaterThan(r, d)) {
                r = me().op_Subtraction(r, d);
            }
        }

        return r;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
      (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
      function (a, n) {
          if (n < 0) {
              return me().op_LeftShift(a, -n);
          }

          n = n & 0x3f;

          if (n > 24) {
              return mscorlib.System.UInt64.op_RightShift(mscorlib.System.UInt64.op_RightShift(a, 24), n - 24);
          }

          var m = (1 << n) - 1;
          var ad = a.data;
          var cr = (ad[2] & m) << (24 - n);
          var ct = ad[2] >>> n;
          var br = (ad[1] & m) << (24 - n);
          var bt = ad[1] >>> n;
          var at = ad[0] >>> n;
          return ctor(at | br, bt | cr, ct);
      });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return ad[0] < bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_LessThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff < 0)
            return true;

        if (adiff > 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff < 0)
            return true;

        if (bdiff > 0)
            return false;

        return ad[0] <= bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return ad[0] > bd[0];
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThanOrEqual",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
        var ad = a.data;
        var bd = b.data;

        var adiff = ad[2] - bd[2];
        if (adiff > 0)
            return true;

        if (adiff < 0)
            return false;

        var bdiff = ad[1] - bd[1];
        if (bdiff > 0)
            return true;

        if (bdiff < 0)
            return false;

        return ad[0] >= bd[0];
    });

    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function () {
        var a = this;
        var ten = me().FromNumber(10);

        var s = "";

        do {
            var r = me().op_Modulus(a, ten);
            s = r.data[0].toString() + s;
            a = me().op_Division(a, ten);
        } while (me().op_GreaterThan(a, minValue()));

        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToHex",
    new JSIL.MethodSignature("System.String", []),
    function () {

        var d = this.data;
        var s = d[0].toString(16);

        if (d[1] > 0 || d[2] > 0) {
            if (s.length < 6)
                s = (new Array(6 - s.length + 1)).join('0') + s;

            s = d[1].toString(16) + s;

            if (d[2] > 0) {
                if (s.length < 12)
                    s = (new Array(12 - s.length + 1)).join('0') + s;

                s = d[2].toString(16) + s;
            }
        }

        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToInt64",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int64"), []),
    function () {
        var d = this.data;
        return mscorlib.System.Int64.Create(d[0], d[1], d[2]);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "Clone",
    new JSIL.MethodSignature($.Type, []),
    function () {
        var d = this.data;
        return ctor(d[0], d[1], d[2]);
    });

    $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function (rhs) {
        return UInt64.op_Equality(this, rhs);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Int32")], [])),
    function (n) {
        if (n < 0)
            throw new Error("cannot construct UInt64 from negative number");

        // only using 48 bits

        var n0 = Math.floor(n);
        return ctor(
            (n0 & 0xffffff),
            (n0 >>> 24) & 0xffffff,
            0);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Double"), [], [])),
    function () {
        return 0x1000000 * (0x1000000 * this.data[2] + this.data[1]) + this.data[0];
    });
});

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

    var ctor =
      function (a, b, c) {
          var r = new mscorlib.System.Int64();
          r.data = [a, b, c];
          return r;
      };

    var mscorlib = JSIL.GetCorlib();
    
    function lazy(f) {
        var state = null;
        return function () {
            if (state === null)
                state = f();
            return state;
        };
    };

    var me = lazy(function () {
        return mscorlib.System.Int64;
    });

    JSIL.Make64BitInt($, ctor, me);

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

    var isNegative = function (a) {
        return mscorlib.System.UInt64.op_GreaterThan(a, signedMaxValue());
    };

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_UnaryNegation",
    (new JSIL.MethodSignature($.Type, [$.Type], [])),
    function (a) {
        return me().op_Addition(me().op_Subtraction(unsignedMaxValue(), a), one());
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Division",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (isNegative(d))
            return me().op_Division(me().op_UnaryNegation(n), me().op_UnaryNegation(d));
        else if (isNegative(n))
            return me().op_UnaryNegation(me().op_Division(me().op_UnaryNegation(n), d));
        else
        // fix return type error
            return mscorlib.System.UInt64.op_Division(n, d).ToInt64();
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_Modulus",
    (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])),
    function (n, d) {
        if (me().op_Equality(d, zero()))
            throw new Error("System.DivideByZeroException");

        if (isNegative(d))
            return me().op_Modulus(me().op_UnaryNegation(n), me().op_UnaryNegation(d));
        else if (isNegative(n))
            return me().op_UnaryNegation(me().op_Modulus(me().op_UnaryNegation(n), d));
        else
        // fix return type error
            return mscorlib.System.UInt64.op_Modulus(n, d);
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_RightShift",
    (new JSIL.MethodSignature($.Type, [$.Type, mscorlib.TypeRef("System.Int32")], [])),
    function (a, n) {
        // Int64 (signed) uses arithmetic shift, UIn64 (unsigned) uses logical shift

        if (n < 0) {
            return me().op_LeftShift(a, -n);
        }

        if (isNegative(a)) {
            return me().op_UnaryNegation(mscorlib.System.UInt64.op_RightShift(a, n));
        }
        else {
            return mscorlib.System.UInt64.op_RightShift(a, n).ToInt64();
        }
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: true }, "op_GreaterThan",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [$.Type, $.Type], [])),
    function (a, b) {
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
    function (a, b) {
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
    function (a, b) {
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
    function (a) {
        return me().op_Equality(this, a);
    });

    // Might need to be implemented externally
    $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature("System.String", []),
    function () {
        var s = "";
        var a = this;
        if (isNegative(this)) {
            s += "-";
            a = me().op_UnaryNegation(this);
        }
        s += mscorlib.System.UInt64.prototype.toString.apply(a);
        return s;
    });

    // Not present in mscorlib
    $.Method({ Static: true, Public: false }, "FromNumber",
    (new JSIL.MethodSignature($.Type, [mscorlib.TypeRef("System.Int32")], [])),
    function (n) {
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
    $.Method({ Static: false, Public: true }, "ToUInt64",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.UInt64"), []),
    function () {
        var d = this.data;
        return mscorlib.System.UInt64.Create(d[0], d[1], d[2]);
    });

    // Not present in mscorlib
    $.Method({ Static: false, Public: true }, "ToNumber",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Double"), [], [])),
    function () {
        var neg = isNegative(this);
        var n = neg ? me().op_UnaryNegation(this) : this;
        var r = 0x1000000 * (0x1000000 * n.data[2] + n.data[1]) + n.data[0]; ;

        if (neg)
            return -r;
        else
            return r;
    });
});

JSIL.MakeStruct("System.ValueType", "System.UInt64", true, [], function ($) {
    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );
});

JSIL.MakeStruct("System.ValueType", "System.Int64", true, [], function ($) {
    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, "int64"
    );
});
