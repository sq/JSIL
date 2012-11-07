"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.ImplementExternals(
  "System.TimeSpan", function ($) {
    var dTicksPerMillisecond = 10000;
    var dTicksPerSecond = 10000000;
    var dTicksPerMinute = 600000000;
    var dTicksPerHour = 36000000000;
    var dTicksPerDay = 864000000000;

    var fromTicks = function (ticks) {
      return JSIL.CreateInstanceOfType(
        $jsilcore.System.TimeSpan.__Type__, "$fromTicks", 
        [ticks]
      );
    };

    var fromDoubleTicks = function (ticks) {
      return $jsilcore.System.TimeSpan.FromTicks(
        $jsilcore.System.Int64.FromNumber(ticks)
      );
    };

    $.Method({Static:true , Public:true }, "FromMilliseconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMilliseconds (value) {
        return fromDoubleTicks(value * dTicksPerMillisecond);
      }
    );

    $.Method({Static:true , Public:true }, "FromSeconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromSeconds (value) {
        return fromDoubleTicks(value * dTicksPerSecond);
      }
    );

    $.Method({Static:true , Public:true }, "FromMinutes", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMinutes (value) {
        return fromDoubleTicks(value * dTicksPerMinute);
      }
    );

    $.Method({Static:true , Public:true }, "FromHours", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromHours (value) {
        return fromDoubleTicks(value * dTicksPerHour);
      }
    );

    $.Method({Static:true , Public:true }, "FromDays", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromDays (value) {
        return fromDoubleTicks(value * dTicksPerDay);
      }
    );

    $.Method({Static:true , Public:true }, "FromTicks", 
      (new JSIL.MethodSignature($.Type, [$.Int64], [])), 
      fromTicks
    );

    $.Method({Static:true , Public:true }, "op_Addition", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Addition (t1, t2) {
        return fromTicks($jsilcore.System.Int64.op_Addition(t1._ticks, t2._ticks));
      }
    );

    $.Method({Static:true , Public:true }, "op_Equality", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_Equality (t1, t2) {
        return t1._ticks === t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_GreaterThan", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_GreaterThan (t1, t2) {
        return t1._ticks > t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_Inequality", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_Inequality (t1, t2) {
        return t1._ticks !== t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_LessThan", 
      (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
      function op_LessThan (t1, t2) {
        return t1._ticks < t2._ticks;
      }
    );

    $.Method({Static:true , Public:true }, "op_Subtraction", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Subtraction (t1, t2) {
        return fromTicks($jsilcore.System.Int64.op_Subtraction(t1._ticks, t2._ticks));
      }
    );

    $.RawMethod(false, "$accumulate", function (multiplier, amount) {
      var tInt64 = $jsilcore.System.Int64;
      var multiplier64 = tInt64.FromNumber(multiplier);
      var amount64 = tInt64.FromNumber(amount);
      var scaled = tInt64.op_Multiplication(multiplier64, amount64);

      if (!this._ticks)
        this._ticks = scaled;
      else
        this._ticks = tInt64.op_Addition(this._ticks, scaled);
    });

    $.RawMethod(false, "$fromTicks", 
      function fromTicks (ticks) {
        this._ticks = ticks;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.Int64], [])), 
      function _ctor (ticks) {
        this._ticks = ticks;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (hours, minutes, seconds) {
        this.$accumulate(dTicksPerHour, hours);
        this.$accumulate(dTicksPerMinute, minutes);
        this.$accumulate(dTicksPerSecond, seconds);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds) {
        this.$accumulate(dTicksPerDay, days);
        this.$accumulate(dTicksPerHour, hours);
        this.$accumulate(dTicksPerMinute, minutes);
        this.$accumulate(dTicksPerSecond, seconds);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds, milliseconds) {
        this.$accumulate(dTicksPerDay, days);
        this.$accumulate(dTicksPerHour, hours);
        this.$accumulate(dTicksPerMinute, minutes);
        this.$accumulate(dTicksPerSecond, seconds);
        this.$accumulate(dTicksPerMillisecond, milliseconds);
      }
    );

    $.Method({Static:false, Public:true }, "get_Days", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Days () {
        return this.$divide(dTicksPerDay);
      }
    );

    $.Method({Static:false, Public:true }, "get_Hours", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Hours () {
        return this.$modDiv(dTicksPerDay, dTicksPerHour);
      }
    );

    $.Method({Static:false, Public:true }, "get_Milliseconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Milliseconds () {
        return this.$modDiv(dTicksPerSecond, dTicksPerMillisecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_Minutes", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Minutes () {
        return this.$modDiv(dTicksPerHour, dTicksPerMinute);
      }
    );

    $.Method({Static:false, Public:true }, "get_Seconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Seconds () {
        return this.$modDiv(dTicksPerMinute, dTicksPerSecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_Ticks", 
      (new JSIL.MethodSignature($.Int64, [], [])), 
      function get_Ticks () {
        return this._ticks;
      }
    );

    $.RawMethod(false, "$modDiv", function $modulus (modulus, divisor) {
      modulus = Math.floor(modulus);
      divisor = Math.floor(divisor);

      var tInt64 = $jsilcore.System.Int64;
      var intModulus = tInt64.FromNumber(modulus);
      var intDivisor = tInt64.FromNumber(divisor);

      var result = tInt64.op_Modulus(this._ticks, intModulus);
      result = tInt64.op_Division(result, intDivisor);
      return result.ToInt32();
    });

    $.RawMethod(false, "$divide", function $divide (divisor) {
      divisor = Math.floor(divisor);

      var tInt64 = $jsilcore.System.Int64;
      var intDivisor = tInt64.FromNumber(divisor);

      var result = tInt64.op_Division(this._ticks, intDivisor);
      return result.ToInt32();
    });

    $.RawMethod(false, "$toNumberDivided", function $toNumberDivided (divisor) {
      divisor = Math.floor(divisor);

      var tInt64 = $jsilcore.System.Int64;
      var intDivisor = tInt64.FromNumber(divisor);

      var integral = tInt64.op_Division(this._ticks, intDivisor);
      var remainder = tInt64.op_Modulus(this._ticks, intDivisor);
      var scaledRemainder = remainder.ToNumber() / divisor;

      var result = integral.ToNumber() + scaledRemainder;
      return result;
    });

    $.Method({Static:false, Public:true }, "get_TotalMilliseconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMilliseconds () {
        return this.$toNumberDivided(dTicksPerMillisecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalSeconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalSeconds () {
        return this.$toNumberDivided(dTicksPerSecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMinutes", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMinutes () {
        return this.$toNumberDivided(dTicksPerMinute);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalHours", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalHours () {
        return this.$toNumberDivided(dTicksPerHour);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalDays", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalDays () {
        return this.$toNumberDivided(dTicksPerDay);
      }
    );
  }
);

JSIL.MakeStruct("System.ValueType", "System.TimeSpan", true, [], function ($) {
  $.Field({Static:false, Public:false}, "_ticks", $.Int64);

  $.Property({Public: true , Static: false}, "Ticks");

  $.Property({Public: true , Static: false}, "Milliseconds");

  $.Property({Public: true , Static: false}, "TotalMilliseconds");

  $.Property({Public: true , Static: false}, "Seconds");

  $.Property({Public: true , Static: false}, "Minutes");

  $.Property({Public: true , Static: false}, "Hours");

  $.Property({Public: true , Static: false}, "Days");

  $.Property({Public: true , Static: false}, "TotalSeconds");

  $.Property({Public: true , Static: false}, "TotalMinutes");
});

JSIL.MakeEnum(
  "System.DateTimeKind", true, {
    Unspecified: 0, 
    Utc: 1, 
    Local: 2
  }, false
);

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.DateTime", true, [], function ($) {
});