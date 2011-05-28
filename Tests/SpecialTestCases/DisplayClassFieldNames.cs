using System;

public static class Program {
    public static void Main (string[] args) {
        int x = 1;
        string y = "y";

        Func<string> a = () => {
            return String.Format("x={0}, y={1}", x, y);
        };

        Console.WriteLine("a()={0}", a());
    }
}