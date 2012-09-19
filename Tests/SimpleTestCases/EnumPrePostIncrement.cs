using System;

public enum Directions {
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}

public static class Program {
    public static void Main (string[] args) {
        var d = Directions.Up;

        Console.WriteLine("{0}", ++d);
        Console.WriteLine("{0}", d);
        Console.WriteLine("{0}", d++);
        Console.WriteLine("{0}", d);
        Console.WriteLine("{0}", --d);
        Console.WriteLine("{0}", d);
        Console.WriteLine("{0}", d--);
        Console.WriteLine("{0}", d);
    }
}