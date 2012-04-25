using System;

public static class Program {
    [Flags]
    public enum FlagsEnum {
        A = 1,
        B = 2,
        C = 4
    }

    public static void Main (string[] args) {
        var a = FlagsEnum.A;

        Console.WriteLine(
            "'{0}' '{1}' '{2}' '{3}'", 
            a, 
            a | FlagsEnum.A,
            a | FlagsEnum.B,
            a | FlagsEnum.B | FlagsEnum.C
        );

        Console.WriteLine(
            "'{0}' '{1}' '{2}'", 
            a & FlagsEnum.A, 
            a & FlagsEnum.B,
            a & (FlagsEnum.A | FlagsEnum.B)
        );
    }
}