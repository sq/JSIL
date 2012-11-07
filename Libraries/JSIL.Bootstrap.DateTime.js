"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.ImplementExternals(
  "System.TimeSpan", function ($) {
    var TicksPerMillisecond, TicksPerSecond, TicksPerMinute, TicksPerHour, TicksPerDay;
    var TempI64A, TempI64B;

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

    $.RawMethod(true, ".cctor", function () {
      var tInt64 = $jsilcore.System.Int64;
      TicksPerMillisecond = tInt64.FromNumber(10000);
      TicksPerSecond = tInt64.FromNumber(10000000);
      TicksPerMinute = tInt64.FromNumber(600000000);
      TicksPerHour = tInt64.FromNumber(36000000000);
      TicksPerDay = tInt64.FromNumber(864000000000);

      TempI64A = new tInt64();
      TempI64B = new tInt64();
    });

    $.Method({Static:true , Public:true }, "FromMilliseconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMilliseconds (value) {
        return fromDoubleTicks(value * TicksPerMillisecond.valueOf());
      }
    );

    $.Method({Static:true , Public:true }, "FromSeconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromSeconds (value) {
        return fromDoubleTicks(value * TicksPerSecond.valueOf());
      }
    );

    $.Method({Static:true , Public:true }, "FromMinutes", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMinutes (value) {
        return fromDoubleTicks(value * TicksPerMinute.valueOf());
      }
    );

    $.Method({Static:true , Public:true }, "FromHours", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromHours (value) {
        return fromDoubleTicks(value * TicksPerHour.valueOf());
      }
    );

    $.Method({Static:true , Public:true }, "FromDays", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromDays (value) {
        return fromDoubleTicks(value * TicksPerDay.valueOf());
      }
    );

    $.Method({Static:true , Public:true }, "FromTicks", 
      (new JSIL.MethodSignature($.Type, [$.Int64], [])), 
      fromTicks
    );

    $.Method({Static:true , Public:true }, "op_Addition", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Addition (t1, t2) {
        return fromTicks($jsilcore.System.Int64.op_Addition(t1._ticks, t2._ticks, TempI64A));
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
        return fromTicks($jsilcore.System.Int64.op_Subtraction(t1._ticks, t2._ticks, TempI64A));
      }
    );

    $.RawMethod(false, "$accumulate", function (multiplier, amount) {
      // FIXME: unnecessary garbage
      var tInt64 = $jsilcore.System.Int64;
      var amount64 = tInt64.FromNumber(amount);
      var scaled = tInt64.op_Multiplication(multiplier, amount64);

      if (!this._ticks)
        this._ticks = scaled;
      else
        this._ticks = tInt64.op_Addition(this._ticks, scaled);

      this.$invalidate();
    });

    $.RawMethod(false, "$fromTicks", 
      function fromTicks (ticks) {
        if (typeof (ticks) === "number")
          throw new Error("Argument must be an Int64");

        this._ticks = ticks.MemberwiseClone();
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [$.Int64], [])), 
      function _ctor (ticks) {
        if (typeof (ticks) === "number")
          throw new Error("Argument must be an Int64");
        
        this._ticks = ticks.MemberwiseClone();
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (hours, minutes, seconds) {
        this.$accumulate(TicksPerHour, hours);
        this.$accumulate(TicksPerMinute, minutes);
        this.$accumulate(TicksPerSecond, seconds);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds) {
        this.$accumulate(TicksPerDay, days);
        this.$accumulate(TicksPerHour, hours);
        this.$accumulate(TicksPerMinute, minutes);
        this.$accumulate(TicksPerSecond, seconds);
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds, milliseconds) {
        this.$accumulate(TicksPerDay, days);
        this.$accumulate(TicksPerHour, hours);
        this.$accumulate(TicksPerMinute, minutes);
        this.$accumulate(TicksPerSecond, seconds);
        this.$accumulate(TicksPerMillisecond, milliseconds);
      }
    );

    $.Method({Static:false, Public:true }, "get_Days", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Days () {
        return this.$divide(TicksPerDay);
      }
    );

    $.Method({Static:false, Public:true }, "get_Hours", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Hours () {
        return this.$modDiv(TicksPerDay, TicksPerHour);
      }
    );

    $.Method({Static:false, Public:true }, "get_Milliseconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Milliseconds () {
        return this.$modDiv(TicksPerSecond, TicksPerMillisecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_Minutes", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Minutes () {
        return this.$modDiv(TicksPerHour, TicksPerMinute);
      }
    );

    $.Method({Static:false, Public:true }, "get_Seconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Seconds () {
        return this.$modDiv(TicksPerMinute, TicksPerSecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_Ticks", 
      (new JSIL.MethodSignature($.Int64, [], [])), 
      function get_Ticks () {
        return this._ticks;
      }
    );

    $.RawMethod(false, "$modDiv", function $modulus (modulus, divisor) {
      var tInt64 = $jsilcore.System.Int64;
      var result = tInt64.op_Modulus(this._ticks, modulus, TempI64A);
      result = tInt64.op_Division(result, divisor, TempI64B);
      return result.ToInt32();
    });

    $.RawMethod(false, "$divide", function $divide (divisor) {
      var tInt64 = $jsilcore.System.Int64;
      var result = tInt64.op_Division(this._ticks, divisor, TempI64A);
      return result.ToInt32();
    });

    $.RawMethod(false, "$toNumberDivided", function $toNumberDivided (divisor) {
      var tInt64 = $jsilcore.System.Int64;
      var integral = tInt64.op_Division(this._ticks, divisor, TempI64A);
      var remainder = tInt64.op_Modulus(this._ticks, divisor, TempI64B);
      var scaledRemainder = remainder.ToNumber() / divisor.valueOf();

      var result = integral.ToNumber() + scaledRemainder;
      return result;
    });

    $.RawMethod(false, "$invalidate", function () {
      this._cachedTotalMs = this._cachedTotalS = null;
    });

    $.Method({Static:false, Public:true }, "get_TotalMilliseconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMilliseconds () {
        if (this._cachedTotalMs)
          return this._cachedTotalMs;
        else
          return this._cachedTotalMs = this.$toNumberDivided(TicksPerMillisecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalSeconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalSeconds () {
        if (this._cachedTotalS)
          return this._cachedTotalS;
        else
          return this._cachedTotalS = this.$toNumberDivided(TicksPerSecond);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMinutes", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMinutes () {
        return this.$toNumberDivided(TicksPerMinute);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalHours", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalHours () {
        return this.$toNumberDivided(TicksPerHour);
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalDays", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalDays () {
        return this.$toNumberDivided(TicksPerDay);
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