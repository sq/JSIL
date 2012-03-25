using System;

public static class Program {
    public enum MyEnum {
        A,
        B,
        C
    }

    public static MyEnum Field;

    public static int f (int x) {
        return x;
    }

    public static void Main (string[] args) {
        Console.WriteLine(Field);
        Field = (MyEnum)f(0);
        Console.WriteLine(Field);
        Field = (MyEnum)f(1);
        Console.WriteLine(Field);
        Field = MyEnum.C;
        Console.WriteLine(Field);
    }
}