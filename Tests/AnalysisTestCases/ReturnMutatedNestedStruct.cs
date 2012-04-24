using System;

public static class Program {
    public static CustomParentType ReturnMutatedArgument (CustomParentType arg, int i) {
        var result = arg;
        result.Nested.Value += i;
        return result;
    }

    public static void Main (string[] args) {
        var a = new CustomParentType(1);
        CustomParentType b = ReturnMutatedArgument(a, 0);
        a.Nested.Value = 2;
        Console.WriteLine("a={0}, b={1}", a, b);
        b = ReturnMutatedArgument(a, 2);
        Console.WriteLine("a={0}, b={1}", a, b);
        a.Nested.Value = 3;
        Console.WriteLine("a={0}, b={1}", a, b);
    }
}

public struct CustomParentType {
    public CustomType Nested;
  
    public CustomParentType (int value) {
        Nested = new CustomType(value);
    }

    public override string ToString () {
        return String.Format("{0}", Nested);
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