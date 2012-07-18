using System;

public static class Program {
    public static void Main (string[] args) {
        Func<int> i = () => 1234;

        Console.WriteLine(i().ToString(null, null));
    }
}