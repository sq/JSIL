using System;
using JSIL;

public static class Program {
    public static int Add (int a, int b) {
        return Verbatim.Expression<int>("a + b");  
    }

    public static void Main (string[] args) {
        Console.WriteLine(Add(1, 3));
        Verbatim.Expression("return");
        Console.WriteLine(2);
    }
}