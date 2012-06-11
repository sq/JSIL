using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234, i2 = 123456;

        Console.WriteLine(String.Format("{0:x}", i));
        Console.WriteLine(String.Format("{0:x}", i2));

        Console.WriteLine(String.Format("{0:x2}", i));
        Console.WriteLine(String.Format("{0:x2}", i2));

        Console.WriteLine(String.Format("{0:x6}", i));
        Console.WriteLine(String.Format("{0:x6}", i2));

        Console.WriteLine(String.Format("{0:X6}", i));
        Console.WriteLine(String.Format("{0:X6}", i2));
    }
}