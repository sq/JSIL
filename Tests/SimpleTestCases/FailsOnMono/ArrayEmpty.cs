using System;

public static class Program {
    public static void Main (string[] args)
    {
        var emptyObj1 = Array.Empty<object>();
        var emptyObj2 = Array.Empty<object>();
        var intObj1 = Array.Empty<int>();
        var intObj2= Array.Empty<int>();

        Console.WriteLine(emptyObj1.GetType());
        Console.WriteLine(intObj1.GetType());
        Console.WriteLine(emptyObj1 == emptyObj2 ? "true" : "false");
        Console.WriteLine(intObj1 == intObj2 ? "true" : "false");
        Console.WriteLine(Equals(emptyObj1, intObj1) ? "true" : "false");
    }
}

public static class A
{
    public static T[] Empty<T>()
    {
        return EmptyArray<T>.Value;
    }

    internal static class EmptyArray<T>
    {
        public static readonly T[] Value = new T[0];
    }
}