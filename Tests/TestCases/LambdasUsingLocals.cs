using System;

public static class Program {
    public static void Main (string[] args) {
        int x = 1;
        string y = "y";

        Func<string> a = () => {
            return String.Format("x={0}, y={1}", x, y);
        };
        Func<int, string> b = (z) => {
            return String.Format("x={0}, y={1}, z={2}", x, y, z);
        };

        Console.WriteLine("a()={0} b()={1}", a(), b(3));
    }
}