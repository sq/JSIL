using System;

public static class Program {
    public enum MyEnum {
        A,
        B
    }

    public static void PrintNullable<T> (T? i) where T : struct {
        if (i.HasValue)
            Console.Write("{0} ", i.Value);
        else
            Console.Write("null ");
    }

    public static void PrintNullables<T> (T? a, T? b) where T : struct {
        PrintNullable(a);
        PrintNullable(b);
        Console.WriteLine();
    }

    public static void PrintNullables<T> (T? a, T? b, T? c) where T : struct {
        PrintNullable(a);
        PrintNullable(b);
        PrintNullable(c);
        Console.WriteLine();
    }

    public static void Main(string[] args)
    {
        int? one = 1, two = 2, three = 3;
        int? nul = null;
        MyEnum? a = MyEnum.A, b = MyEnum.B;

        PrintNullables(a, b, null);
        PrintNullables(a + one, a - one);
        PrintNullables(a + nul, a - nul);
    }
}