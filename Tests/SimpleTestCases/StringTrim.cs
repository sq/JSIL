using System;

public static class Program {
    public static string TrimIt (string s) {
        return s.Trim();
    }

    public static void Main (string[] args) {
        Console.WriteLine(TrimIt("abc"));
        Console.WriteLine(TrimIt(" abc"));
        Console.WriteLine(TrimIt("abc "));
        Console.WriteLine(TrimIt(" abc "));
    }
}