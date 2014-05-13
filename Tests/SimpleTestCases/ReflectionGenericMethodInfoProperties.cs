using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        Write(typeof(NonGeneric).GetMethod("NonGenericMethod"));
        Write(typeof(NonGeneric).GetMethod("GenericMethod"));
        Write(typeof(NonGeneric).GetMethod("GenericMethod").MakeGenericMethod(new [] {typeof(string)}));

        Write(typeof(Generic1<,>).GetMethod("NonGenericMethod"));
        Write(typeof(Generic1<,>).GetMethod("GenericMethodBoth"));
        Write(typeof(Generic1<,>).GetMethod("GenericMethodFirst"));
        Write(typeof(Generic1<,>).GetMethod("GenericMethodSecond"));
        Write(typeof(Generic1<,>).GetMethod("GenericMethodExternal"));
        Write(typeof(Generic1<,>).GetMethod("GenericMethodExternal").MakeGenericMethod(new[] { typeof(string) }));

        Write(typeof(Generic2<>).BaseType.GetMethod("NonGenericMethod"));
        Write(typeof(Generic2<>).BaseType.GetMethod("GenericMethodBoth"));
        Write(typeof(Generic2<>).BaseType.GetMethod("GenericMethodFirst"));
        Write(typeof(Generic2<>).BaseType.GetMethod("GenericMethodSecond"));
        Write(typeof(Generic2<>).BaseType.GetMethod("GenericMethodExternal"));
        Write(typeof(Generic2<>).BaseType.GetMethod("GenericMethodExternal").MakeGenericMethod(new[] { typeof(string) }));

        Write(typeof(Generic2<>).GetMethod("NonGenericMethodDerived"));
        Write(typeof(Generic2<>).GetMethod("GenericMethodDerived"));
        Write(typeof(Generic2<>).GetMethod("GenericMethodExternalDerived"));
        Write(typeof(Generic2<>).GetMethod("GenericMethodExternalDerived").MakeGenericMethod(new[] { typeof(string) }));

        Write(typeof(Generic2<int>).BaseType.GetMethod("NonGenericMethod"));
        Write(typeof(Generic2<int>).BaseType.GetMethod("GenericMethodBoth"));
        Write(typeof(Generic2<int>).BaseType.GetMethod("GenericMethodFirst"));
        Write(typeof(Generic2<int>).BaseType.GetMethod("GenericMethodSecond"));
        Write(typeof(Generic2<int>).BaseType.GetMethod("GenericMethodExternal"));
        Write(typeof(Generic2<int>).BaseType.GetMethod("GenericMethodExternal").MakeGenericMethod(new[] { typeof(string) }));

        Write(typeof(Generic2<int>).GetMethod("NonGenericMethodDerived"));
        Write(typeof(Generic2<int>).GetMethod("GenericMethodDerived"));
        Write(typeof(Generic2<int>).GetMethod("GenericMethodExternalDerived"));
        Write(typeof(Generic2<int>).GetMethod("GenericMethodExternalDerived").MakeGenericMethod(new[] { typeof(string) }));

        Write(typeof(Generic2<>).MakeGenericType(new Type[] { typeof(Generic2<>) }).GetMethod("NonGenericMethodDerived"));
    }


    public static void Write(MethodInfo methodInfo)
    {
        Console.WriteLine("IsGenericMethod: {0}", methodInfo.IsGenericMethod);
        Console.WriteLine("IsGenericMethodDefinition: {0}", methodInfo.IsGenericMethodDefinition);
        Console.WriteLine("ContainsGenericParameters: {0}", methodInfo.ContainsGenericParameters);
        Console.WriteLine();
    }
 }


public class NonGeneric 
{
    public void GenericMethod<T>(T input)
    {
    }

    public void NonGenericMethod(string input)
    {
    }
}

public class Generic1<T,V>
{
    public void NonGenericMethod()
    {
    }
    public void GenericMethodBoth(T input, V input2)
    {
    }
    public void GenericMethodFirst(T input)
    {
    }
    public void GenericMethodSecond(T input)
    {
    }
    public void GenericMethodExternal<U>(U input)
    {
    }
}

public class Generic2<V> : Generic1<string, V>
{
    public void NonGenericMethodDerived()
    {
    }
    public void GenericMethodDerived(V input)
    {
    }
    public void GenericMethodExternalDerived<U>(U input)
    {
    }
}
