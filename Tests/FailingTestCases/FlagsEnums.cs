using System;

public static class Program {
    [Flags]
    public enum FlagsEnum {
        A = 1,
        B = 2,
        C = 4
    }

    public const FlagsEnum E = FlagsEnum.A | FlagsEnum.C;

    public static void Main (string[] args) {
        Console.WriteLine("'{0}' '{1}' '{2}'", FlagsEnum.A, FlagsEnum.A | FlagsEnum.B | FlagsEnum.C, E);
        Console.WriteLine("{0} {1}", (int)FlagsEnum.A, (int)E);
        Console.WriteLine("'{0}' '{1}'", Enum.Parse(typeof(FlagsEnum), "C"), Enum.Parse(typeof(FlagsEnum), "7"));
    }
}