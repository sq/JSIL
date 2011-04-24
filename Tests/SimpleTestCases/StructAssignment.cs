using System;

public static class Program { 
    public static void Main (string[] args) {
        var a = new CustomType(1);
        CustomType b = a;
        a.Value = 2;
        Console.WriteLine("a={0}, b={1}", a, b);
        b = a;
        Console.WriteLine("a={0}, b={1}", a, b);
        a.Value = 3;
        Console.WriteLine("a={0}, b={1}", a, b);
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