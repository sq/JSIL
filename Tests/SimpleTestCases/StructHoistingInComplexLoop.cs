using System;
using System.Collections.Generic;

public class Sprite {
    public static int Count = 0;

    public readonly int Index;

    public Sprite () {
        Index = Count++;
    }

    public override string ToString () {
        return String.Format("{0}", Index);
    }
}

public struct PointPx {
    public readonly int X, Y;

    public PointPx (int x, int y) {
        X = x;
        Y = y;
    }

    public override string ToString () {
        return String.Format("{{{0}, {1}}}", X, Y);
    }
}

public static class Program {
    public static Sprite[] FillDictionaryWithPoints (Dictionary<Sprite, PointPx> dictionary) {
        var result = new List<Sprite>();

        int top = 0, bottom = 4;
        int left = 0, right = 4;

        for (int y = top; y < bottom; y++) {
            for (int x = left; x < right; x++) {
                Sprite s = new Sprite();

                if (s != null)
                    dictionary[s] = new PointPx(x * 16 + 8, y * 16 + 8);

                result.Add(s);
            }
        }

        return result.ToArray();
    }

    public static void Main (string[] args) {
        var dict = new Dictionary<Sprite, PointPx>();
        var sprites = FillDictionaryWithPoints(dict);

        foreach (var sprite in sprites)
            Console.WriteLine("{0}={1}", sprite, dict[sprite]);
    }
}