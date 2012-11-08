using System;

public class Program {
    public static void Main() {
        var ticks = TimeSpan.FromTicks(500000009);
        var seconds = TimeSpan.FromSeconds(5003);
        var minutes = TimeSpan.FromMinutes(451);
        var hours = TimeSpan.FromHours(379);
        var days = TimeSpan.FromDays(1075);

        Util.PrintTimeSpan("Days + Ticks", days + ticks);
        Util.PrintTimeSpan("Hours + Minutes", hours + minutes);
        Util.PrintTimeSpan("Hours + Minutes + Seconds", hours + minutes + seconds);

        // FIXME: Why is this value's TotalSeconds off by 1???
        // Util.PrintTimeSpan("Days - Ticks", days - ticks);
        Util.PrintTimeSpan("Hours - Minutes", hours - minutes);
        Util.PrintTimeSpan("Hours - Minutes - Seconds", hours - minutes - seconds);
    }
}