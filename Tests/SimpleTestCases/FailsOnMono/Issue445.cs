using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var aOut = (object)new Implementor2<object, string>();
        var bOut = (IOutT<string>)aOut;
        bOut.Get();
        var cOut = (IOutT<object>)aOut;
        cOut.Get();

        var aIn = (object)new Implementor2<string, object>();
        var bIn = (IInT<string>)aIn;
        bIn.Set(null);
        var cIn = (IInT<object>)aIn;
        cIn.Set(null);
    }
}

public interface IOutT<out T>
{
    T Get();
}

public interface IInT<in T>
{
    void Set(T value);
}


public class Implementor<T1> : IOutT<T1>, IInT<T1>
{
    T1 IOutT<T1>.Get()
    {
        Console.WriteLine("Implementor<" + typeof(T1).Name + ">.Get");
        return default(T1);
    }

    void IInT<T1>.Set(T1 value)
    {
        Console.WriteLine("Implementor<" + typeof(T1).Name + ">.Set");
    }
}

public class Implementor2<T1, T2> : Implementor<T1>, IOutT<T2>, IInT<T2>
{
    T2 IOutT<T2>.Get()
    {
        Console.WriteLine("Implementor2<" + typeof(T1).Name + "," + typeof(T2).Name + ">.Get");
        return default(T2);
    }

    void IInT<T2>.Set(T2 value)
    {
        Console.WriteLine("Implementor2<" + typeof(T1).Name + "," + typeof(T2).Name + ">.Set");
    }
}