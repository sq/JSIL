using System;

public enum Enum1 {
    Value0 = 0,
    Value1 = 1,
}

public static class Program {
    public static void Main (string[] args) {
        var v1 = Enum1.Value1;
        Console.WriteLine((v1 & Enum1.Value1) == Enum1.Value1 ? "true" : "false");
    }
}