using System;

public static class Program {
    public static long GetSize () {
        return 1024;
    }

    public static void Main (string[] args) {
        var a = new int[GetSize()];

        Console.WriteLine(a.Length);
        Console.WriteLine(a[16]);
    }
}