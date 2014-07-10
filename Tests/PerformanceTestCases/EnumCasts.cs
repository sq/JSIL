using System;
using System.Collections.Generic;

public static class Program {
    static MyEnum[] enums = new MyEnum[32];


    public static void Main () {
        int numIterations = 2048000;

        for (var i = 0; i < numIterations; i++)
            CastTest();
    }

    public static void CastTest () {
        for (var i = 0; i < 32; i++)
            enums[i] = (MyEnum)i;
    }
}

public enum MyEnum : int {
    A,
    B = 3,
    C = 7,
    D = 15
}