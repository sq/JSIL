using System;

public static class Program {
    private enum MyEnum {
        A,
        B,
        C
    }

    public static void Main (string[] args) {
        Console.WriteLine((MyEnum)0);
        Console.WriteLine((MyEnum)1);
        Console.WriteLine(MyEnum.C);
    }
}