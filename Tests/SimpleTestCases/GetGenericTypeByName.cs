using System;
using JSIL.Meta;

public class CustomType<T> {
}

public static class Program {
    [JSReplacement("'JSIL.Core'")]
    public static string GetAssemblyName () {
        return "mscorlib";
    }

    public static void Main (string[] args) {
        Console.WriteLine(System.Type.GetType("CustomType`1[System.String]"));
        Console.WriteLine(System.Type.GetType("CustomType`1[[System.String," + GetAssemblyName() + "]]"));
    }
}