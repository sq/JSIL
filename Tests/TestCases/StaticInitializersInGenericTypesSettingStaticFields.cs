using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var value = SomeGenericClass<int>.InitializedValue;
        Console.WriteLine("Woo, found a value of {0}", value);
    }
}

public class SomeGenericClass<T>
{
    public static readonly int InitializedValue;

    static SomeGenericClass()
    {
        InitializedValue = 100;
    }
}