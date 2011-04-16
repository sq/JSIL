using System;

public class CustomType {
    public readonly int Value;

    public CustomType (int value) {
        Value = value;
    }

    public static CustomType operator - (CustomType v) {
        return new CustomType(-v.Value);
    }

    public static CustomType operator + (CustomType lhs, CustomType rhs) {
        return new CustomType(lhs.Value + rhs.Value);
    }

    public override string ToString () {
        return Value.ToString();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(2);
        var b = new CustomType(3);
        Console.WriteLine(a + b);
        Console.WriteLine(-b);
    }
}