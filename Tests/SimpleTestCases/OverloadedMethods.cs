using System;

public static class Program { 
    public static void Main (string[] args) {
        OverloadedMethod();
        OverloadedMethod(1);
        OverloadedMethod("a");
        OverloadedMethod(new CustomType(3));
    }

    public static void OverloadedMethod () {
        Console.WriteLine("OverloadedMethod(<void>)");
    }

    public static void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }

    public static void OverloadedMethod (string s) {
        Console.WriteLine("OverloadedMethod(<string>)");
    }

    public static void OverloadedMethod (CustomType ct) {
        OverloadedMethod(ct.Value);
    }
}

public class CustomType {
    public readonly int Value;

    public CustomType (int value) {
        Value = value;
    }

    public override string ToString () {
        return String.Format("CustomType({0})", Value);
    }
}