using System;

public static class Program {
    private static void PrintRounded (double d) {
        Console.WriteLine(Math.Round(d, 7));
    }

    public static void Main (string[] args) {
        PrintRounded(Math.Floor(-0.5));
        PrintRounded(Math.Floor(0.5));
        PrintRounded(Math.Ceiling(-0.5));
        PrintRounded(Math.Ceiling(0.5));
        PrintRounded(Math.Abs(-0.5));
        PrintRounded(Math.Sqrt(2));
        PrintRounded(Math.Sin(0.5));
        PrintRounded(Math.Cos(0.5));
        PrintRounded(Math.Asin(0.5));
        PrintRounded(Math.Acos(0.5));
        PrintRounded(Math.Tan(0.5));
        PrintRounded(Math.Atan(0.5));
        PrintRounded(Math.Atan2(0.5, -1.5));
        PrintRounded(Math.Log(100));
        PrintRounded(Math.Log10(100));
        PrintRounded(Math.Log(100, 10));
        PrintRounded(Math.Log(100, 5));
        PrintRounded(Math.Sign(-5));
        PrintRounded(Math.Pow(5, 3));
        PrintRounded(Math.Exp(3));
    }
}