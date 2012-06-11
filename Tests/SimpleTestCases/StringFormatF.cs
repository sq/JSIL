using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234;
        float f = 12.34f;

        Console.WriteLine(String.Format("{0:f}", i));
        Console.WriteLine(String.Format("{0:f}", f));

        Console.WriteLine(String.Format("{0:f4}", i));
        Console.WriteLine(String.Format("{0:f4}", f));
    }
}