using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new int[20];

        for (var i = 0; i < a.Length; i++)
            a[i] = i;

        foreach (var i in a)
            Console.WriteLine(i);
    }
}