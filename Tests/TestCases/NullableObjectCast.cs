using System;

public static class Program {
    public static void PrintBool (bool b) {
        Console.WriteLine(b ? 1 : 0);
    }

    public static void Main(string[] args) {
        var o = new object[] { 
            new Int32?(3),
            new Int32?(),
            "a"
        };

        PrintBool(o[0] is Int32?);
        PrintBool(o[1] is Int32?);
        PrintBool(o[2] is Int32?);
        PrintBool(o[2] is String);

        var ni = (o[0] as Int32?);
        PrintBool(ni.HasValue);
        Console.WriteLine(ni.Value);
    }
}