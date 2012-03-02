using System;
using System.Reflection;

public static class Program {
    public static void MethodA () {
    }

    public static void MethodB () {
    }

    public static void Main (string[] args) {
        Console.WriteLine(typeof(Program));

        var methods = typeof(Program).GetMethods(
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public
        );

        foreach (var method in methods)
            Console.WriteLine(method.Name);
    }
}