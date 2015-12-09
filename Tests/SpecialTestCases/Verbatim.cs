using System;
using JSIL;

public static class Program {
    public static int Add (int a, int b)
    {
        return Builtins.IsJavascript ? Verbatim.Expression("a + b").AssumeType<int>() : 0;
    }

    public static void Main (string[] args) {
        Console.WriteLine(Add(1, 3));
        if (Builtins.IsJavascript)
          Verbatim.Expression("return");
        Console.WriteLine(2);
    }
}