using System;

public static class Program { 
    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(a);
        Console.WriteLine("a={0}, b={1}", a, b);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    // I had no idea this was even possible until I saw mscorlib do it :o
    public CustomType (CustomType rhs) {
        this = rhs;
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}