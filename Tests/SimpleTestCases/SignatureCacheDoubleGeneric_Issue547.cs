using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = OuterGeneric<InnerGeneric<Class1>>.Create();
        var b = OuterGeneric<InnerGeneric<Class2>>.Create();
    }
}

public class OuterGeneric<T>
{
    public static OuterGeneric<T> Create()
    {
        Console.WriteLine(typeof(T).GetGenericArguments()[0].Name);
        return new OuterGeneric<T>();
    }
}

public class InnerGeneric<T>
{
}

public class Class1
{
}

public class Class2
{
}