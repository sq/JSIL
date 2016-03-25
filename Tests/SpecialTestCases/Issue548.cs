using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        var obj1 = (dynamic)Verbatim.Expression("{}");
        var obj2 = (dynamic)Verbatim.Expression("{obj1: JSON.stringify($0)}", obj1);
        Console.WriteLine(Verbatim.Expression("JSON.stringify($0)", obj2));
    }
}