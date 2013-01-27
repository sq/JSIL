using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("Signed");
        {
            int x = int.MinValue;
            x--;
            Console.WriteLine(x);
        }
        {
            int x = int.MaxValue;
            x++;
            Console.WriteLine(x);
        }
        {
            int x = int.MinValue;
            x -= 5;
            Console.WriteLine(x);
        }
        {
            int x = int.MaxValue;
            x += 5;
            Console.WriteLine(x);
        }

        Console.WriteLine("Unsigned");
        {
            uint x = uint.MinValue;
            x--;
            Console.WriteLine(x);
        }
        {
            uint x = uint.MaxValue;
            x++;
            Console.WriteLine(x);
        }
        {
            uint x = uint.MinValue;
            x -= 5;
            Console.WriteLine(x);
        }
        {
            uint x = uint.MaxValue;
            x += 5;
            Console.WriteLine(x);
        }
    }
}