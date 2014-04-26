using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = (object)new Implementor<object>();
        try
        {
            var b = (IInterface<string>) a;
            b.Get();
        }
        catch (Exception)
        {
            Console.WriteLine("Expected exception");
        }
    }

    public static IEnumerable<T> GetTypedArray<T>(Func<IEnumerable<T>> func)
    {
        return func();
    }
}

public interface IInterface<out T>
{
    T Get();
}

public class Implementor<T> : IInterface<T>
{
    public T Get()
    {
        Console.WriteLine(typeof(T).Name);
        return default(T);
    }
}