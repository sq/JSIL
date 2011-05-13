using System;

public static class Program {
    public static void Increment (ref int x) {
        x += 1;
    }

    public static void Main (string[] args) {
        Func<int, int> inc = (i) => {
            Increment(ref i);
            return i;
        };

        Console.WriteLine("{0}, {1}", inc(1), inc(2));
    }
}