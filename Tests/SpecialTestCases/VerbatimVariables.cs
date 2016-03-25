using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        Verbatim.Expression("print($0)", "hello");
        var a = 2;
        var b = 5;
        Console.WriteLine(Verbatim.Expression("$0 + $1", a, b));
        int i = (int)(Verbatim.Expression("$0 + $1", a, b) ?? 0);
        Console.WriteLine(i);
    }
}