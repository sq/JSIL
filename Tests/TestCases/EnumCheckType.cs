using System;

public static class Program {
    private enum MyEnum {
        A,
        B,
        C
    }

    public static void CheckIsEnum<T> (T value) {
        Console.WriteLine((value is MyEnum) ? 1 : 0);
    }

    public static void Main (string[] args) {
        object o = MyEnum.A;
        MyEnum e = MyEnum.B;
        int i = (int)MyEnum.C;

        CheckIsEnum(o);
        CheckIsEnum(e);
        CheckIsEnum(i);
    }
}