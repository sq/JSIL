using System;
using JSIL.Meta;

public class CustomType {
}

public static class Program {
    [JSReplacement("'JSIL.Core'")]
    public static string GetAssemblyName () {
        return "mscorlib";
    }

    public static void Main (string[] args) {
        Console.WriteLine(System.Type.GetType("CustomType"));
        Console.WriteLine(System.Type.GetType("System.String"));
        Console.WriteLine(System.Type.GetType("System.String," + GetAssemblyName()));
        Console.WriteLine(System.Type.GetType("System.String,  " + GetAssemblyName() + "  "));
    }
}