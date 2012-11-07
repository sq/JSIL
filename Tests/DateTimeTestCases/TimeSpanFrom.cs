using System;

public class Program {
    public static void PrintTimeSpan (string caption, TimeSpan ts) {
      Console.WriteLine("{0}:", caption);
      Console.WriteLine("d={0} h={1} m={2} s={3}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
      Console.WriteLine("td={0:0000.0000} th={1:0000.0000} tm={2:0000.0000} ts={3:0000.0000}", ts.TotalDays, ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
    }
  
    public static void Main() {
      PrintTimeSpan("Ticks", TimeSpan.FromTicks(500000009));
      PrintTimeSpan("Seconds", TimeSpan.FromSeconds(5003));
      PrintTimeSpan("Minutes", TimeSpan.FromMinutes(451));
      PrintTimeSpan("Hours", TimeSpan.FromHours(379));
      PrintTimeSpan("Days", TimeSpan.FromDays(1075));
    }
}