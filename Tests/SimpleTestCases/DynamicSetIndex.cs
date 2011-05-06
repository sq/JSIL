using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic array = new int[] { 1, 2, 3 };
        Console.WriteLine("{0}", array[1]);
        array[1] = 4;
        Console.WriteLine("{0}", array[1]);
    }
}