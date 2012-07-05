using System;
using System.Globalization;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Int32.Parse("0", NumberStyles.HexNumber));
        Console.WriteLine(Int32.Parse("32", NumberStyles.HexNumber));
    }
}