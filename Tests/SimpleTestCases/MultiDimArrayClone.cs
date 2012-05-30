using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new int[2, 2];
        var b = (int[,])a.Clone();

        a[0,0] = 1;
        b[1,1] = 2;

        foreach (var i in a)
            Console.WriteLine(i);

        foreach (var i in b)
            Console.WriteLine(i);
    }
}