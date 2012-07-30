using System;

public static class Program {
    public static int Truncate (double value) {
        return (int)value;
    }

    public static void Main (string[] args) {
        Console.WriteLine("{0}", Truncate(1.75 * 0.5));
        Console.WriteLine("{0}", Truncate(-1.75 * 0.5));
        Console.WriteLine("{0}", Truncate(1.75 * 0.75));
        Console.WriteLine("{0}", Truncate(-1.75 * 0.75));
    }
}