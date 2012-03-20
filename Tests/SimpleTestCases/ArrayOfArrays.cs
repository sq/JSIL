using System;

public static class Program {
    public static void Main (string[] args) {
        Func<int, int> f = (i) => i;

        var arr = new int[3][];

        // We must use a delegate here to prevent the C# compiler from collapsing this into a single array initializer
        arr[f(0)] = new int[] { 1 };
        arr[f(1)] = new int[] { 2, 3 };
        arr[f(2)] = new int[] { 4, 5, 6 };

        foreach (var a in arr)
            foreach (var b in a)
                Console.WriteLine(b);
    }
}