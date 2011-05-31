using System;

public static class Program {
    public enum SimpleEnum {
        A,
        B = 3,
        C,
        D = 10
    }

    public enum ByteEnum : byte {
        A,
        B = 3,
    }

    public static void Main (string[] args) {
        Console.WriteLine("{0} {1} {2}", SimpleEnum.A, SimpleEnum.B, SimpleEnum.C);
        Console.WriteLine("{0} {1}", ByteEnum.A, ByteEnum.B);
        Console.WriteLine("{0} {1} {2}", (int)SimpleEnum.A, (int)SimpleEnum.B, (int)SimpleEnum.C);
        Console.WriteLine("{0} {1}", (int)ByteEnum.A, (int)ByteEnum.B);
        Console.WriteLine("{0} {1}", Enum.Parse(typeof(SimpleEnum), "C"), Enum.Parse(typeof(SimpleEnum), "3"));
    }
}