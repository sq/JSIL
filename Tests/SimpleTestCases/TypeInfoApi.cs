using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(IntrospectionExtensions.GetTypeInfo(typeof(Exception)).DeclaredConstructors.First(obj => obj.GetParameters().Length == 2));
        Console.WriteLine(typeof(List<int>).GetTypeInfo().GenericTypeArguments[0] == typeof(int) ? "true" : "false");
    }
}