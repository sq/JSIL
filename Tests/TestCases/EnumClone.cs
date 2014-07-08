using System;

public static class Program {
    public enum MyEnum {
        A,
        B,
        C
    }

    public static MyEnum? NE;
    public static MyEnum E;

    public static void Main (string[] args) {
        MyEnum? e1 = MyEnum.A;
        MyEnum? e2 = null;

        Console.WriteLine(e1.Value.ToString());
        Console.WriteLine(e2.GetValueOrDefault(MyEnum.B).ToString());

        NE = e2;
        E = e1.Value;

        Console.WriteLine((int)(E));

        NE = e1;
        E = e2.GetValueOrDefault(MyEnum.C);

        Console.WriteLine((int)(NE.Value));
        Console.WriteLine((int)(E));
    }
}