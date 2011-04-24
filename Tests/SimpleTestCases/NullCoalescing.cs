using System;
using System.Collections.Generic;

public class CustomType {
    public readonly int Value;

    public CustomType (int value) {
        Value = value;
    }

    public override string ToString () {
        return Value.ToString();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        CustomType c = null;

        Console.WriteLine(a ?? b);
        Console.WriteLine(b ?? a);
        Console.WriteLine(a ?? c);
        Console.WriteLine(b ?? c);
    }
}