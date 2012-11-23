using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        var c = a;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
        c.Nested.Value = 3;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
    }
}

public struct NestedCustomType {
    public int Value;

    public NestedCustomType (int value) {
        Value = value;
    }

    public override string ToString () {
        return Value.ToString();
    }
}

public struct CustomType {
    public NestedCustomType Nested;

    public CustomType (int value) {
        Nested = new NestedCustomType(value);
    }
    
    public override string ToString () {
        return Nested.ToString();
    }
}