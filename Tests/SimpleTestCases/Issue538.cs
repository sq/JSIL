using System;

public static class Program {
    public static void Main (string[] args) {
        var a = GetNull() ?? 1;
        Console.WriteLine(a);
    }

    public static int? GetNull () {
        return null;
    }
}