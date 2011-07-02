using System;

public static class Program {
    public static CustomType ReturnArgument (CustomType arg) {
        return arg;
    }

    public static CustomType ReturnIncrementedArgument (CustomType arg) {
        ++arg.Value;
        return arg;
    }

    public static void Main (string[] args) {
        var a = new CustomType(1);
        CustomType b = ReturnArgument(ReturnIncrementedArgument(ReturnArgument(a)));
        Console.WriteLine("a={0}, b={1}", a, b);
        a.Value = 3;
        Console.WriteLine("a={0}, b={1}", a, b);
        b = ReturnArgument(ReturnArgument(a));
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