using System;

public static class Program {
    public static void Main (string[] args) {
        {
            byte x = 100;
            x -= 150;
            Console.WriteLine(x);
        }
        {
            byte x = 100;
            x += 200;
            Console.WriteLine(x);
        }
    }
}