using System;

public static class Program {
    public enum SimpleEnum : int {
        A = 1,
        B = 2,
        C
    }

    public static void Main (string[] args) {
        var array = new int[] { 0, 1, 2, 3 };
        var index = SimpleEnum.B;

        Console.WriteLine("{0} {1} {2}", array[(int)SimpleEnum.A], array[(int)index], array[(int)SimpleEnum.C]);
        array[0] = 5;
        array[(int)index] = 6;
        array[(int)SimpleEnum.C] = 4;
        Console.WriteLine("{0} {1} {2}", array[(int)SimpleEnum.A], array[(int)index], array[(int)SimpleEnum.C]);
    }
}