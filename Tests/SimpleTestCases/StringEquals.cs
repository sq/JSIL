using System;

public static class Program {
    public static void PrintEquals (object a, object b) {
        Console.WriteLine(a.Equals(b) ? 1 : 0);
    }

    public static void Main (string[] args) {
        PrintEquals("asd", "asd");
    }
}