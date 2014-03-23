using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new[]
            {
                TestEnum.One,
                default(TestEnum),
                TestEnum.Two
            };

        Console.WriteLine(a[0]);
        Console.WriteLine(a[1]);
        Console.WriteLine(a[2]);

        var b = new[]
            {
                TestEnumWithDefault.One,
                default(TestEnumWithDefault),
                TestEnumWithDefault.Default,
                TestEnumWithDefault.Two
            };

        Console.WriteLine(b[0]);
        Console.WriteLine(b[1]);
        Console.WriteLine(b[2]);
        Console.WriteLine(b[3]);
    }
}

public enum TestEnum {
    One = 1,
    Two = 2
}

public enum TestEnumWithDefault {
    One = 1,
    Default = 0,
    Two = 2
}