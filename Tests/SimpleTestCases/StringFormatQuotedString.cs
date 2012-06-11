using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234;
        double d = 1234.56;

        Console.WriteLine(String.Format("{0:00.00 'hello'}, {1:00.00 'hello'}", i, d));
        Console.WriteLine(String.Format("{0:00.00 \"hello\"}, {1:00.00 \"hello\"}", i, d));
        Console.WriteLine(String.Format("{0:00.00 'hel\"lo'}, {1:00.00 'hel\"lo'}", i, d));
        Console.WriteLine(String.Format("{0:00.00 \"hel'lo\"}, {1:00.00 \"hel'lo\"}", i, d));
    }
}