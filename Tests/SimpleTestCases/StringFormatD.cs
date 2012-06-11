using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234, i2 = 123456;

        Console.WriteLine(String.Format("{0:d}", i));
        Console.WriteLine(String.Format("{0:d}", i2));

        Console.WriteLine(String.Format("{0:d2}", i));
        Console.WriteLine(String.Format("{0:d2}", i2));

        Console.WriteLine(String.Format("{0:d6}", i));
        Console.WriteLine(String.Format("{0:d6}", i2));
    }
}