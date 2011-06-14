using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new int[20];

        foreach (var i in a)
            Console.WriteLine(i);
    }
}