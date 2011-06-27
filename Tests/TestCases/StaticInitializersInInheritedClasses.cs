using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var instanceOfClass = new DerivedClass();
    }
}

public class BaseClass<T>
{
    static BaseClass()
    {
        Console.WriteLine("Hello Base");
    }
}

public class DerivedClass : BaseClass<int>
{
    static DerivedClass()
    {
        Console.WriteLine("Hello Derived");
    }
}