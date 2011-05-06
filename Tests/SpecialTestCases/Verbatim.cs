using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(1);
        JSIL.Verbatim.Expression("return");
        Console.WriteLine(2);
    }
}