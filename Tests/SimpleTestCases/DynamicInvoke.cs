using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic one = (Func<int>)(
            () => 1
        );
        dynamic doubleInt = (Func<int, int>)(
            (int x) => x * 2
        );

        Console.WriteLine("{0} {1}", one(), doubleInt(1));
    }
}