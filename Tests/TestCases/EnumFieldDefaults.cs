using System;

public static class Program {
    public enum MyEnum {
        A,
        B,
        C
    }

    public static MyEnum Field1;
    public static MyEnum Field2 = MyEnum.A;

    public static void Main (string[] args) {
        Console.WriteLine(Field1);
        Console.WriteLine(Field2);
    }
}