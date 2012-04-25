using System;

public static class Program {
    public enum MyEnum {
        A,
        B
    }

    public static void PrintBool (bool b) {
        Console.WriteLine(b ? 1 : 0);
    }

    public static void Main(string[] args) {
        Func<int> zero = () => 0,
            one = () => 1,
            two = () => 2;

        Func<MyEnum?> a = () => MyEnum.A,
            b = () => MyEnum.B,
            nul = () => null;

        PrintBool((MyEnum)zero() == a());
        PrintBool((MyEnum)one() == a());
        PrintBool((MyEnum)one() == b());
        PrintBool((MyEnum)two() == nul());
    }
}