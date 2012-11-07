using System;

public class Program {
    public static void PrintTimeSpan (string caption, TimeSpan ts) {
        Console.WriteLine("{0}:", caption);
        Console.WriteLine("d={0} h={1} m={2} s={3}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        Console.WriteLine("td={0:0000.0000} th={1:0000.0000} tm={2:0000.0000} ts={3:0000.0000}", ts.TotalDays, ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
    }
  
    public static void Main() {
        var ticks = TimeSpan.FromTicks(500000009);
        var seconds = TimeSpan.FromSeconds(5003);
        var minutes = TimeSpan.FromMinutes(451);
        var hours = TimeSpan.FromHours(379);
        var days = TimeSpan.FromDays(1075);

        PrintTimeSpan("Days + Ticks", days + ticks);
        PrintTimeSpan("Hours + Minutes", hours + minutes);
        PrintTimeSpan("Hours + Minutes + Seconds", hours + minutes + seconds);
    }
}