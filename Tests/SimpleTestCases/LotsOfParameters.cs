using System;

public static class Program {
    public static void Main (string[] args) {
        LotsOfArgs(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
    }

    public static void LotsOfArgs (
        int a, int b, int c, int d, int e, int f, int g, int h,
        int i, int j, int k, int l, int m, int n, int o, int p
    ) {
        Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);
        Console.WriteLine("{0} {1} {2} {3}", e, f, g, h);
        Console.WriteLine("{0} {1} {2} {3}", i, j, k, l);
        Console.WriteLine("{0} {1} {2} {3}", m, n, o, p);
    }
}