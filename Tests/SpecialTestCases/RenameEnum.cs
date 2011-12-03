using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("{0} {1} {2}", MyEnum.A, MyEnum.B, MyEnum.C.GetType());
    }
}

[JSChangeName("RenamedEnum")]
public enum MyEnum {
    A,
    B,
    C
}