using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Util {
    public static void PrintTimeSpan (string caption, TimeSpan ts) {
        Console.WriteLine("{0}:", caption);
        PrintTimeSpan(ts);
    }

    public static void PrintTimeSpan (TimeSpan ts) {
        Console.WriteLine("d={0} h={1} m={2} s={3}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        Console.WriteLine("td={0:0000.0000} th={1:0000.0000} tm={2:0000.0000} ts={3:0000.0000}", ts.TotalDays, ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
    }
}