using System;

public static class Program {
    public static void Main (string[] args) {
        Func<int, int, int> one = (x, y) => (int)(x / (y == 0 ? 0.00001 : y));
        Func<float, float, float> two = (x, y) => (int)(x / (y == 0 ? 0.00001 : y));

        Console.WriteLine(
            "{0} {1} {2} {3}",
            one(1, 2), one(1, 0),
            two(1, 2), two(1, 0)
        );
    }
}