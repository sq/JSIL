using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("abc".PadLeft(5));
        Console.WriteLine("abc".PadLeft(3));
        Console.WriteLine("abc".PadLeft(2));

        Console.WriteLine("abc".PadRight(5));
        Console.WriteLine("abc".PadRight(3));
        Console.WriteLine("abc".PadRight(2));

        Console.WriteLine("abc".PadLeft(5, 'd'));
        Console.WriteLine("abc".PadLeft(3, 'd'));
        Console.WriteLine("abc".PadLeft(2, 'd'));

        Console.WriteLine("abc".PadRight(5, 'd'));
        Console.WriteLine("abc".PadRight(3, 'd'));
        Console.WriteLine("abc".PadRight(2, 'd'));
    }
}