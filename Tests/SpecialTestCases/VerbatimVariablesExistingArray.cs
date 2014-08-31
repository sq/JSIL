using System;
using JSIL;

public static class Program {
    static object[] A1 = new object[] { "hello" };
    static object[] A2 = new object[] { 2, 5 };

    public static void Main (string[] args) {
        Verbatim.Expression("print($0)", A1);
        Console.WriteLine(Verbatim.Expression("$0 + $1", A2));
    }
}