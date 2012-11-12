using System;
using JSIL.Meta;

public class CustomType<T, U, V> {
}

public static class Program {
    [JSReplacement("'JSIL.Core'")]
    public static string GetAssemblyName () {
        return "mscorlib";
    }

    public static string GetQualified (string typeName) {
        return "[" + typeName + "," + GetAssemblyName() + "]";
    }

    public static void Main (string[] args) {
        Console.WriteLine(System.Type.GetType("CustomType`3[System.String, System.Int32, System.Single]"));
        Console.WriteLine(System.Type.GetType(
            "CustomType`3[" + GetQualified("System.String") + "," +
            GetQualified("System.Int32") + "," +
            GetQualified("System.Single") + 
        "]"));
    }
}