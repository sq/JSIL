using System;

public static class Program {
    public enum MyEnum {
        A = 0,
        B = 1,
    }

    public static MyEnum? f (MyEnum? x) {
        if (x.HasValue)
            return x;
        else
            return null;
    }

    public static void Main (string[] args) {
        Console.WriteLine((int)f(MyEnum.A));
        Console.WriteLine((int)f(MyEnum.B));
    }
}