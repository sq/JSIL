using System;

public static class Program {
    [Flags]
    public enum SimpleEnum {
        A = 0,
        B = 3,
        C,
        D = 10,
        E = 0
    }

    public static void Main (string[] args) {
        const SimpleEnum a = SimpleEnum.B;
        SimpleEnum b = SimpleEnum.E;

        Console.WriteLine("{0} {1}", a, b);
        Console.WriteLine("{0} {1}", a & SimpleEnum.B, b & SimpleEnum.D);
    }
}