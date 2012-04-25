using System;

public static class Program {
    public static string LowerIt (string s) {
        return s.ToLower();
    }

    public static string UpperIt (string s) {
        return s.ToUpper();
    }

    public static void Main (string[] args) {
        Console.WriteLine(LowerIt("abc"));
        Console.WriteLine(LowerIt("Abc"));
        Console.WriteLine(LowerIt("ABC"));
        Console.WriteLine(UpperIt("abc"));
        Console.WriteLine(UpperIt("Abc"));
        Console.WriteLine(UpperIt("ABC"));
    }
}