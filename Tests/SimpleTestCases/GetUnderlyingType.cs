using System;

public static class Program {
    public static void Main(string[] args) {
        Console.WriteLine("{0} {1}", typeof(double?), Nullable.GetUnderlyingType(typeof(double?)));
    }
}