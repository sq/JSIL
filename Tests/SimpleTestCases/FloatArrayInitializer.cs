using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new float[] { 1.2f, 5.7f, 100, -5 };

        Console.WriteLine(a.Length);
        Console.WriteLine(a[2]);
    }
}