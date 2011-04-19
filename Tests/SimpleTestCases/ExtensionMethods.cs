using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.ExtensionMethod();
        instance.ExtensionMethod(5);
    }

    public static void ExtensionMethod (this CustomType ct) {
        Console.WriteLine("ExtensionMethod(<CustomType>)");
    }

    public static void ExtensionMethod (this CustomType ct, int i) {
        Console.WriteLine("ExtensionMethod(<CustomType>, <int>)");
    }
}

public class CustomType {
}