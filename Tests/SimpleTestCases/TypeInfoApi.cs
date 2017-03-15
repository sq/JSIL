using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class Program {
    public static void Main (string[] args)
    {
        var assertion = IntrospectionExtensions.GetTypeInfo(typeof(Exception)).DeclaredConstructors
            .Any(obj => obj.GetParameters().Length == 2
                        && obj.GetParameters()[0].ParameterType == typeof(string)
                        && obj.GetParameters()[1].ParameterType == typeof(Exception));
        if (!assertion)
        {
            throw new Exception("Invalid result for DeclaredConstructors");
        }

        Console.WriteLine(assertion ? "true" : "false");

        Console.WriteLine(typeof(List<int>).GetTypeInfo().GenericTypeArguments[0] == typeof(int) ? "true" : "false");
    }
}