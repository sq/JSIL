using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        var c = a;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
        c.Value = 3;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
    }
}

public class CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}