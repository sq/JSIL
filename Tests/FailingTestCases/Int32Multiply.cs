using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("Signed");
        {
            int x = int.MinValue;
            x = x * x;
            Console.WriteLine(x);
        }
        {
            int x = int.MaxValue;
            x = x * x;
            Console.WriteLine(x);
        }

        Console.WriteLine("Unsigned");
        {
            uint x = uint.MaxValue;
            x = x * x;
            Console.WriteLine(x);
        }
    }
}