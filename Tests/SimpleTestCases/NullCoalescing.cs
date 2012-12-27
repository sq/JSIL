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

public struct CustomStruct
{
    public readonly int Value;

    public CustomStruct(int value) {
        Value = value;
    }

    public override string ToString() {
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

        CustomStruct? a1 = new CustomStruct(1);
        CustomStruct? b1 = new CustomStruct(2);
        CustomStruct? c1 = null;

        Console.WriteLine(a1 ?? b1);
        Console.WriteLine(b1 ?? a1);
        Console.WriteLine(a1 ?? c1);
        Console.WriteLine(b1 ?? c1);
    }
}