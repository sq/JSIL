using System;

public static class Program {
    public static void Main (string[] args) {
        {
            sbyte x = -100;
            x -= 90;
            Console.WriteLine(x);
        }
        {
            sbyte x = 100;
            x += 90;
            Console.WriteLine(x);
        }

        {
            short x = -32700;
            x -= 200;
            Console.WriteLine(x);
        }
        {
            short x = 32700;
            x += 200;
            Console.WriteLine(x);
        }
    }
}