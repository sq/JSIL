using System;

public static class Program {
    public static void Main (string[] args) {
        // [y, x], not [x, y]!
        var a = new string[5, 10];
        var h = a.GetLength(0);
        var w = a.GetLength(1);

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                a[y, x] = String.Format("x={0}, y={1}", x, y);

        foreach (var s in a)
            Console.WriteLine(s);
    }
}