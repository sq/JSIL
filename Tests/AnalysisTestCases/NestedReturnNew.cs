using System;

public static class Program {
    public static CustomType ReturnArgument (CustomType arg) {
        return arg;
    }

    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        var c = new CustomType(3);
        var d = CustomType.AddThree(a, b, c);
        Console.WriteLine("a={0}, b={1}, c={2}, d={3}", a, b, c, d);
        d = ReturnArgument(CustomType.AddThree(a, b, c));
        Console.WriteLine("a={0}, b={1}, c={2}, d={3}", a, b, c, d);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    public static CustomType AddThree (CustomType a, CustomType b, CustomType c) {
        return a + (b + c);
    }

    public static CustomType operator + (CustomType lhs, CustomType rhs) {
        return new CustomType(lhs.Value + rhs.Value);
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}