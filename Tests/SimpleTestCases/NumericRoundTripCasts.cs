using System;

public static class Program {
    public static Int32 I () {
        return 1024000;
    }

    public static Int32 J () {
        return -1024000;
    }

    public static Int16 K () {
        return -1028;
    }

    public static void Main (string[] args) {
        Console.WriteLine((Int32)(Int64)I());
        Console.WriteLine((Int32)(Int16)I());
        Console.WriteLine((Int32)(float)I());

        Console.WriteLine((Int32)(Int64)J());
        Console.WriteLine((Int32)(Int16)J());
        Console.WriteLine((Int32)(Int64)J());
        Console.WriteLine((Int32)(float)J());

        Console.WriteLine((Int32)(UInt64)J());
        Console.WriteLine((Int32)(UInt16)J());
        Console.WriteLine((Int32)(UInt32)J());

        Console.WriteLine((Int16)(Int64)K());
        Console.WriteLine((UInt16)(Int64)K());
    }
}