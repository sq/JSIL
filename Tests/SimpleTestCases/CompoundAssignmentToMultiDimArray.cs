using System;

public static class Program {
    public static void Main (string[] args) {
        // [y, x], not [x, y]!
        var a = new int[4, 4];
        var h = a.GetLength(0);
        var w = a.GetLength(1);

        a[0, 0] = 1;
        a[0, 1] = 2;
        a[1, 0] = 2;

        for (var y = 0; y < (h - 1); y++)
            for (var x = 0; x < (w - 1); x++)
                a[x + 1, y + 1] += a[x, y];

        foreach (var i in a)
            Console.WriteLine(i);
    }
}