using System;

public static class Program {
    public static string LowerIt (string s) {
        return s.ToLower();
    }

    public static string UpperIt (string s) {
        return s.ToUpper();
    }

    public static string LowerItInv (string s)
    {
      return s.ToLowerInvariant();
    }

    public static string UpperItInv(string s)
    {
      return s.ToUpperInvariant();
    }



    public static void Main (string[] args) {
        Console.WriteLine(LowerIt("abc"));
        Console.WriteLine(LowerIt("Abc"));
        Console.WriteLine(LowerIt("ABC"));
        Console.WriteLine(UpperIt("abc"));
        Console.WriteLine(UpperIt("Abc"));
        Console.WriteLine(UpperIt("ABC"));

        Console.WriteLine(LowerItInv("abc"));
        Console.WriteLine(LowerItInv("Abc"));
        Console.WriteLine(LowerItInv("ABC"));
        Console.WriteLine(UpperItInv("abc"));
        Console.WriteLine(UpperItInv("Abc"));
        Console.WriteLine(UpperItInv("ABC"));


    }
}