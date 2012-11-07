"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.ImplementExternals(
  "System.TimeSpan", function ($) {
    var TicksPerMillisecond = 10000;
    var TicksPerSecond = 10000000;
    var TicksPerMinute = 600000000;
    var TicksPerHour = 36000000000;
    var TicksPerDay = 864000000000;

    $.Method({Static:true , Public:true }, "FromMilliseconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMilliseconds (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * TicksPerMillisecond);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromSeconds", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromSeconds (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * TicksPerSecond);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromMinutes", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromMinutes (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * TicksPerMinute);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromHours", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromHours (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * TicksPerHour);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromDays", 
      (new JSIL.MethodSignature($.Type, [$.Double], [])), 
      function FromDays (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value * TicksPerDay);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "FromTicks", 
      (new JSIL.MethodSignature($.Type, [$.Int64], [])), 
      function FromTicks (value) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = Math.floor(value);
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "op_Addition", 
      (new JSIL.MethodSignature($.Type, [$.Type, $.Type], [])), 
      function op_Addition (t1, t2) {
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = t1._ticks + t2._ticks;
        return result;
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
        var result = Object.create(System.TimeSpan.prototype);
        result._ticks = t1._ticks - t2._ticks;
        return result;
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
        this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * hours)));
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds) {
        this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32, 
            $.Int32, $.Int32, 
            $.Int32
          ], [])), 
      function _ctor (days, hours, minutes, seconds, milliseconds) {
        this._ticks = 10000 * (milliseconds + 1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
      }
    );

    $.Method({Static:false, Public:true }, "get_Days", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Days () {
        return Math.floor((this._ticks / 10000000) / (60 * 60 * 24));
      }
    );

    $.Method({Static:false, Public:true }, "get_Hours", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Hours () {
        return Math.floor((this._ticks / 10000000) / (60 * 60)) % 24;
      }
    );

    $.Method({Static:false, Public:true }, "get_Milliseconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Milliseconds () {
        return Math.floor(this._ticks / 10000) % 1000;
      }
    );

    $.Method({Static:false, Public:true }, "get_Minutes", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Minutes () {
        return Math.floor((this._ticks / 10000000) / 60) % 60;
      }
    );

    $.Method({Static:false, Public:true }, "get_Seconds", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Seconds () {
        return Math.floor(this._ticks / 10000000) % 60;
      }
    );

    $.Method({Static:false, Public:true }, "get_Ticks", 
      (new JSIL.MethodSignature($.Int64, [], [])), 
      function get_Ticks () {
        return this._ticks;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMilliseconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMilliseconds () {
        return this._ticks / TicksPerMillisecond;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalSeconds", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalSeconds () {
        return this._ticks / TicksPerSecond;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalMinutes", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalMinutes () {
        return this._ticks / TicksPerMinute;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalHours", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalHours () {
        return this._ticks / TicksPerHour;
      }
    );

    $.Method({Static:false, Public:true }, "get_TotalDays", 
      (new JSIL.MethodSignature($.Double, [], [])), 
      function get_TotalDays () {
        return this._ticks / TicksPerDay;
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