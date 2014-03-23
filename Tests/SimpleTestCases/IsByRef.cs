using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(typeof(int).IsByRef ? 1 : 0);
        Console.WriteLine(typeof(object).IsByRef ? 1 : 0);
        Console.WriteLine(typeof(Program).GetMethod("Method").GetParameters()[0].ParameterType.IsByRef ? 1 : 0);
    }

    public static void Method (ref int i) {
    }
}