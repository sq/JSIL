using System;

public struct Struct {
    public int X, Y;

    public override string ToString () {
        return String.Format("{0}, {1}", X, Y);
    }
}

public static class Program {
    public static void Main (string[] args) {
        // [y, x], not [x, y]!
        var a = new Struct[4, 6];
        var h = a.GetLength(0);
        var w = a.GetLength(1);

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                a[y, x] = new Struct {
                    X = x,
                    Y = y
                };

        foreach (var s in a)
            Console.WriteLine(s);
    }
}