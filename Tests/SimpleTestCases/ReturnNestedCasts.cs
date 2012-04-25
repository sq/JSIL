using System;

public static class Program {
    public static Random Random = new Random();

    public static int RandIntRange (int min, int max) {
        int result = (int)(Random.NextDouble() * (max - min) + min);
        return result;
    }

    public static void Main (string[] args) {
        var i1 = RandIntRange(0, 10);
        var i2 = RandIntRange(0, 10);
        Console.WriteLine("{0} {1}",
            i1 - ((float)i1), i2 - ((float)i2)
        );
    }
}