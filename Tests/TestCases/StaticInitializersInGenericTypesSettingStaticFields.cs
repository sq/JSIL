using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var instanceOfClass = new SomeGenericClass<int>();
        var value = instanceOfClass.GetMeYourValue();
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

    public int GetMeYourValue()
    {
        return SomeGenericClass<T>.InitializedValue;
    }
}