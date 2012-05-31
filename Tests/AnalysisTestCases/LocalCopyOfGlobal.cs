using System;

public static class Program {
    public static CustomType A = new CustomType(1), B = new CustomType(2);
    public static CustomType Field;

    public static CustomType ReturnArgument (CustomType arg) {
        return arg;
    }

    public static void StoreArgument (CustomType value) {
        Field = value;
    }
    
    public static void PrintArgument (CustomType value) {
        Console.WriteLine("{0}", value);
    }

    public static void Main (string[] args) {
        var a = A;
        var b = ReturnArgument(B);
        var c = B;
        var d = B;
        var e = B;

        PrintArgument(a);
        PrintArgument(b);
        PrintArgument(c);

        StoreArgument(d);
        PrintArgument(Field);

        c.Value = 3;

        PrintArgument(ReturnArgument(a));
        PrintArgument(b);
        PrintArgument(c);

        Field = e;
        PrintArgument(Field);
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