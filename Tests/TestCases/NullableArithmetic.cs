using System;

public static class Program {
    public static void PrintNullable (int? i) {
        if (i.HasValue)
            Console.Write("{0} ", i.Value);
        else
            Console.Write("null ");
    }

    public static void PrintNullables (int? a, int? b) {
        PrintNullable(a);
        PrintNullable(b);
        Console.WriteLine();
    }

    public static void PrintNullables (int? a, int? b, int? c) {
        PrintNullable(a);
        PrintNullable(b);
        PrintNullable(c);
        Console.WriteLine();
    }

    public static void Main(string[] args)
    {
        int? one = 1, two = 2, three = 3;
        int? nul = null;

        PrintNullables(one, two, null);
        PrintNullables(one + one, one - one);
        PrintNullables(one + nul, one - nul);
        PrintNullables(nul + nul, nul - nul);
    }
}