using System;

public class Program {
    public static void Main() {
        Util.PrintTimeSpan("Ticks", new TimeSpan(500000009));
        Util.PrintTimeSpan("Seconds", new TimeSpan(0, 0, 5003));
        Util.PrintTimeSpan("Minutes", new TimeSpan(0, 451, 0));
        Util.PrintTimeSpan("Hours", new TimeSpan(379, 0, 0));
        Util.PrintTimeSpan("Days", new TimeSpan(1075, 0, 0, 0));
    }
}