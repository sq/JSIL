using System;
using JSIL.Meta;

public static class Program {
    public static CustomType IncrementArgument (CustomType arg) {
        arg.Value += 1;
        return arg;
    }

    public static void Main (string[] args) {
        CustomType a = new CustomType(1), b = new CustomType(2);
        Console.WriteLine("a={0}, b={1}", a, b);
        b = a;
        Console.WriteLine("a={0}, b={1}", a, b);
        b = new CustomType(3);
        Console.WriteLine("a={0}, b={1}", a, b);
    }
}

public struct CustomType {
    public int Value;
  
    [JSReplacement("($this.Value = $value, $this)")]
    public CustomType (int value) {
        Value = value;
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}