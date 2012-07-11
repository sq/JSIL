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

    public static SimpleEnum ReturnEnum (SimpleEnum value) {
        return value;
    }

    public static void Main (string[] args) {
        const SimpleEnum b = SimpleEnum.B;
        SimpleEnum e = SimpleEnum.E;

        Console.WriteLine("{0} {1}", b, e);
        Console.WriteLine("{0} {1}", b & ReturnEnum(SimpleEnum.B), e & ReturnEnum(SimpleEnum.D));
    }
}