using System;

public static class Program {
    public static CustomParentType IncrementArgumentValue (CustomParentType arg) {
        ++(arg.Nested.Value);
        return arg;
    }

    public static void Main (string[] args) {
        var a = new CustomParentType(1);
        var b = a;
        a.Nested.Value = 2;
        Console.WriteLine("a={0}, b={1}", a, b);
        IncrementArgumentValue(a);
        Console.WriteLine("a={0}, b={1}", a, b);
        b = IncrementArgumentValue(a);
        Console.WriteLine("a={0}, b={1}", a, b);
        a.Nested.Value = 3;
        Console.WriteLine("a={0}, b={1}", a, b);
    }
}

public struct CustomParentType {
    public CustomType Nested;
  
    public CustomParentType (int value) {
        Nested = new CustomType(value);
    }

    public override string ToString () {
        return String.Format("{0}", Nested);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}