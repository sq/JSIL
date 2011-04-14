using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new SimpleType(0);
        var b = new SimpleType("b");
    }
}

public class SimpleType {
    public SimpleType (int value) {
        Console.WriteLine("SimpleType()");
    }

    public SimpleType (string value) {
        Console.WriteLine("SimpleType()");
    }
}