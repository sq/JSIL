using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(1);
        var b = new CustomType(2);
        var c = a;

        Console.WriteLine("{0}", a.Equals(a) ? 1 : 0);
        Console.WriteLine("{0}", a.Equals(b) ? 1 : 0);
        Console.WriteLine("{0}", a.Equals(c) ? 1 : 0);
        Console.WriteLine("{0}", a == a ? 1 : 0);
        Console.WriteLine("{0}", a == b ? 1 : 0);
        Console.WriteLine("{0}", a == c ? 1 : 0);
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    public bool Equals (CustomType rhs) {
        return Value == rhs.Value;
    }

    public static bool operator == (CustomType lhs, CustomType rhs) {
        return lhs.Equals(rhs);
    }

    public static bool operator != (CustomType lhs, CustomType rhs) {
        return !lhs.Equals(rhs);
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}