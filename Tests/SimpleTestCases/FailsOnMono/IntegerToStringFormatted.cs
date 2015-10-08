using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234;

        Console.WriteLine(i.ToString("00"));
        Console.WriteLine(i.ToString("0000"));
        Console.WriteLine(i.ToString("000000"));

        Console.WriteLine(i.ToString("##"));
        Console.WriteLine(i.ToString("####"));
        Console.WriteLine(i.ToString("######"));
    }
}