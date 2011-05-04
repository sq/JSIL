using System;

public static class Program {
    public static void Increment (ref int x) {
        x += 1;
    }

    public static void Main (string[] args) {
        var a = new SimpleType(0);

        Console.WriteLine("a = {0}", a.Value);
        Increment(ref a.Value);
        Console.WriteLine("a = {0}", a.Value);
    }
}

public class SimpleType {
    public int Value;

    public SimpleType (int value) {
        Value = value;
    }
}