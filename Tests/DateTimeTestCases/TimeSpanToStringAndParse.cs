using System;

public class Program {
    public static void PrintAndParse (TimeSpan ts) {
        var stringified = ts.ToString();
        var parsed = TimeSpan.Parse(stringified);
        Console.WriteLine("{0} -> {1}", stringified, parsed.ToString());
    }

    public static void Main() {
        var ticks = TimeSpan.FromTicks(500000009);
        var seconds = TimeSpan.FromSeconds(5003);
        var minutes = TimeSpan.FromMinutes(451);
        var hours = TimeSpan.FromHours(379);
        var days = TimeSpan.FromDays(1075);
        var multiple = new TimeSpan(1, 2, 3, 4, 5);

        PrintAndParse(ticks);
        PrintAndParse(seconds);
        PrintAndParse(minutes);
        PrintAndParse(hours);
        PrintAndParse(days);
        PrintAndParse(multiple);
    }
}