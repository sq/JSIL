using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234;
        float f = 12.34f;

        Console.WriteLine(String.Format("{0,1}", i));
        Console.WriteLine(String.Format("{0,1}", f));

        Console.WriteLine(String.Format("{0,4}", i));
        Console.WriteLine(String.Format("{0,4}", f));

        Console.WriteLine(String.Format("{0,6}", i));
        Console.WriteLine(String.Format("{0,6}", f));

        Console.WriteLine(String.Format("{0,-1}", i));
        Console.WriteLine(String.Format("{0,-1}", f));

        Console.WriteLine(String.Format("{0,-6}", i));
        Console.WriteLine(String.Format("{0,-6}", f));
    }
}