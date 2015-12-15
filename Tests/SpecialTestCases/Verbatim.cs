using System;
using JSIL;

public static class Program {
    public static int Add (int a, int b) {
        return (int)(Verbatim.Expression("a + b") ?? 0);  
    }

    public static void Main (string[] args) {
        Console.WriteLine(Add(1, 3));
        Verbatim.Expression("return");
        Console.WriteLine(2);
    }
}