using System;

public static class Program { 
    public static void Main (string[] args) {
        CustomType a = new CustomType(1), b = new CustomType(2), c = a;
        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
        a *= b;
        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
        c.Value = 16;
        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    public static CustomType operator * (CustomType lhs, CustomType rhs) {
        return new CustomType(lhs.Value * rhs.Value);
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}