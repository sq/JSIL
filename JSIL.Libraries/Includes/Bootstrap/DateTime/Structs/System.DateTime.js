JSIL.ImplementExternals("System.DateTime", function ($) {
    $.RawMethod(false, "$fromLocalMilliseconds", function (msSince1970) {
        this.dateData = $jsilcore.System.UInt64.op_Multiplication(
          $jsilcore.System.UInt64.FromInt32(msSince1970),
          $jsilcore.System.UInt64.FromInt32(10000)
        );
        this.kind = $jsilcore.System.DateTimeKind.Local;
    });

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function _ctor(ticks) {
          this.dateData = ticks.ToUInt64();
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
      }
    );

    $.Method({ Static: false, Public: false }, ".ctor",
      (new JSIL.MethodSignature(null, [$.UInt64], [])),
      function _ctor(dateData) {
          this.dateData = dateData;
          this.kind = $jsilcore.System.DateTimeKind.Unspecified;
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
            $jsilcore.System.DateTime.__Type__, "$fromLocalMilliseconds", [JSIL.Host.getTime()]
          );
      }
    );

    $.Method({ Static: true, Public: true }, "get_UtcNow",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [], [])),
      function get_UtcNow() {
          // FIXME
          return JSIL.CreateInstanceOfType(
            $jsilcore.System.DateTime.__Type__, "$fromLocalMilliseconds", [JSIL.Host.getTime()]
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