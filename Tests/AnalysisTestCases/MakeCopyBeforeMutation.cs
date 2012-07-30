using System;

public static class Program {
    public static CustomType ReturnMutatedArgument (CustomType arg, int i) {
        var copy = arg;
        arg.Value += i;
        Console.WriteLine("copy={0}, arg={1}", copy, arg);
        return arg;
    }

    public static void Main (string[] args) {
        var a = new CustomType(1);
        CustomType b = ReturnMutatedArgument(a, 0);
        a.Value = 2;
        Console.WriteLine("a={0}, b={1}", a, b);
        b = ReturnMutatedArgument(a, 2);
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