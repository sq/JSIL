using System;

public static class Program {
    public static CustomType ReturnArgument (CustomType arg) {
        return arg;
    }

    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        var c = a + b;
        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
        c = ReturnArgument(a + b);
        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
        Console.WriteLine("{0}", a + c);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    public static CustomType operator + (CustomType lhs, CustomType rhs) {
        return new CustomType(lhs.Value + rhs.Value);
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}