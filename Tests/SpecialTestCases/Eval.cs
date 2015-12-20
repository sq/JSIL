using System;
using JSIL;

public static class Program {
    public static int Foo () {
        if (Builtins.IsJavascript) {
            return Builtins.Eval("2").AssumeType<int>();
        }
        return 1;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Foo());
    }
}