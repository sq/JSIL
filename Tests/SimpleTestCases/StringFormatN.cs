using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234;
        float f = 12.34f;
        double d = 12345.0, d2 = 123456.0, d3 = 1234567.0;

        Console.WriteLine(String.Format("{0:n}", i));
        Console.WriteLine(String.Format("{0:n}", f));

        Console.WriteLine(String.Format("{0:n4}", i));
        Console.WriteLine(String.Format("{0:n4}", f));

        Console.WriteLine(String.Format("{0:n4}", d));
        Console.WriteLine(String.Format("{0:n4}", d2));
        Console.WriteLine(String.Format("{0:n4}", d3));
    }
}