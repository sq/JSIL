using System;

public class Program {
    public static void Main() {
        Util.PrintTimeSpan("Ticks", TimeSpan.FromTicks(500000009));
        Util.PrintTimeSpan("Seconds", TimeSpan.FromSeconds(5003));
        Util.PrintTimeSpan("Minutes", TimeSpan.FromMinutes(451));
        Util.PrintTimeSpan("Hours", TimeSpan.FromHours(379));
        Util.PrintTimeSpan("Days", TimeSpan.FromDays(1075));
        Util.PrintTimeSpan("Fractional Seconds", TimeSpan.FromSeconds(0.333));
        Util.PrintTimeSpan("Fractional Minutes", TimeSpan.FromMinutes(0.333));
        Util.PrintTimeSpan("Fractional Hours", TimeSpan.FromHours(0.333));
    }
}