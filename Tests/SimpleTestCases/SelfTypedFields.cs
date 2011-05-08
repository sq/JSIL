using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new CustomType());
        Console.WriteLine(new CustomType(1));
        Console.WriteLine(CustomType.A);
        Console.WriteLine(CustomType.B);
    }
}

public struct CustomType {
    public int Value;

    public static readonly CustomType A = new CustomType(1);
    public static CustomType B;

    public CustomType (int value) {
        Value = value;
    }

    public override string ToString () {
        return String.Format("Value={0}", Value);
    }
}