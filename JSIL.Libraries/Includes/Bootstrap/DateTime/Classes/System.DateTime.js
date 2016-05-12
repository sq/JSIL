JSIL.ImplementExternals("System.DateTime", function ($) {
    $.RawMethod(false, "$fromUnixMilliseconds", function (msSince1970, boolIsUtc) {
      this.dateData = $jsilcore.System.UInt64.op_Multiplication(
        $jsilcore.System.UInt64.op_Addition(
          $jsilcore.System.UInt64.FromNumber(msSince1970),
          $jsilcore.System.UInt64.FromNumber(62135596800000)),
        $jsilcore.System.UInt64.FromInt32(10000));
        this.kind = boolIsUtc ? $jsilcore.System.DateTimeKind.Utc : $jsilcore.System.DateTimeKind.Local;
    });

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function _ctor(ticks) {
          this.dateData = ticks.ToUInt64();
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Int64, $jsilcore.TypeRef("System.DateTimeKind")], [])),
      function _ctor(ticks, kind) {
          this.dateData = ticks.ToUInt64();
          this.kind = kind;
      }
    );

    $.Method({ Static: false, Public: false }, ".ctor",
      (new JSIL.MethodSignature(null, [$.UInt64], [])),
      function _ctor(dateData) {
          this.dateData = dateData;
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
      }
    );

    $.Method({ Static: false, Public: true }, "ToBinary",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function ToBinary() {
          // FIXME: this.kind is not properly copied on MemberwiseClone
          if (this.kind === undefined)
              this.kind = $jsilcore.System.DateTimeKind.Unspecified;
          return ($jsilcore.System.Int64.op_BitwiseOr(this.dateData, $jsilcore.System.Int64.op_LeftShift(this.kind, 62)));
      }
    );

    $.Method({ Static: true, Public: true }, "FromBinary",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [$.Int64], [])),
      function FromBinary(dateData) {
          var ticks = $jsilcore.System.Int64.op_BitwiseAnd(dateData,
                                                           $jsilcore.System.Int64.Create(16777215, 16777215, 16383) /* 0x3FFFFFFFFFFFFFFF */);
          var kind = $jsilcore.System.Int64.op_RightShift(dateData, 62);

          return new System.DateTime(ticks, $jsilcore.System.DateTimeKind.$Cast(kind));
      }
    );

    var normalYearDays = [
      0, 31, 59,
      90, 120, 151,
      181, 212, 243,
      273, 304, 334,
      365
    ];
    var leapYearDays = [
      0, 31, 60,
      91, 121, 152,
      182, 213, 244,
      274, 305, 335,
      366
    ];

    function isLeapYear(year) {
        if (((year % 100) !== 0) || ((year % 400) === 0)) {
            return (year % 4) === 0;
        }
        return false;
    };

    function ymdToTicks(year, month, day) {
        if ((year < 1) || (year > 9999))
            throw new System.ArgumentException("year");

        if ((month < 1) || (month > 12))
            throw new System.ArgumentException("month");

        var days = isLeapYear(year) ? leapYearDays : normalYearDays;
        var daysThisMonth = days[month] - days[month - 1];

        if ((day < 1) || (day > daysThisMonth))
            throw new System.ArgumentException("day");

        year -= 1;

        var yearDays = year * 365;
        var leapYearDayOffset = ((year / 4) | 0) - ((year / 100) | 0) + ((year / 400) | 0);
        var monthDays = days[month - 1];

        var totalDays = $jsilcore.System.UInt64.FromInt32(
          (yearDays + leapYearDayOffset + monthDays + (day - 1)) | 0
        );
        var ticksPerDay = $jsilcore.System.UInt64.Parse("864000000000");

        var result = $jsilcore.System.UInt64.op_Multiplication(
          totalDays,
          ticksPerDay
        );

        return result;
    };

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32,
            $.Int32
      ], [])),
      function _ctor(year, month, day) {
          this.dateData = ymdToTicks(year, month, day);
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [
            $.Int32, $.Int32,
            $.Int32, $jsilcore.TypeRef("System.Globalization.Calendar")
      ], [])),
      function _ctor(year, month, day, calendar) {
          // FIXME: calendar
          this.dateData = ymdToTicks(year, month, day);
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
      }
    );

    $.Method({ Static: true, Public: true }, "get_Now",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [], [])),
      function get_Now() {
          // FIXME
          return JSIL.CreateInstanceOfType(
            $jsilcore.System.DateTime.__Type__, "$fromUnixMilliseconds", [JSIL.Host.getTime() - JSIL.Host.getTimezoneOffsetInMilliseconds(), false]
          );
      }
    );

    $.Method({ Static: true, Public: true }, "get_UtcNow",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [], [])),
      function get_UtcNow() {
          // FIXME
          return JSIL.CreateInstanceOfType(
            $jsilcore.System.DateTime.__Type__, "$fromUnixMilliseconds", [JSIL.Host.getTime(), true]
          );
      }
    );

    $.Method({ Static: false, Public: true }, "ToLongTimeString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ToLongTimeString() {
          // FIXME
          return "ToLongTimeString";
      }
    );

});

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.DateTime", true, [], function ($) {
});