using System;

public enum Directions {
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}

public static class Program {
    public static Directions Direction;

    public static void Main (string[] args) {
        Direction = Directions.Up;
        Console.WriteLine("{0}", Direction);

        Direction += 2;
        Console.WriteLine("{0}", Direction);

        Console.WriteLine("{0}", Direction -= 2);
        Console.WriteLine("{0}", Direction);
    }
}