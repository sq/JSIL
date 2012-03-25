using System;

public static class Program {
    private enum MyEnum {
        A,
        B,
        C
    }

    public static int f (int x) {
        return x;
    }

    public static void Main (string[] args) {
        Console.WriteLine((MyEnum)f(0));
        Console.WriteLine((MyEnum)f(1));
        Console.WriteLine(MyEnum.C);
    }
}