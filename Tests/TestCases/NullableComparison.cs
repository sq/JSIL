using System;

public static class Program {
    public static void PrintNullablesEqual (int? a, int? b) {
        Console.WriteLine((a == b) ? 1 : 0);
    }

    public static void Main(string[] args)
    {
        int? one = 1, two = 2;
        int? nul = null;

        PrintNullablesEqual(one, one);
        PrintNullablesEqual(one, two);
        PrintNullablesEqual(one + one, two);
        PrintNullablesEqual(one + nul, two);
        PrintNullablesEqual(one + nul, nul);
    }
}