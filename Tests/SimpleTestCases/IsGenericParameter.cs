using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(typeof(int).IsGenericParameter ? 1 : 0);
    }
}