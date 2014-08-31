using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(new GenericClass<A>(new A()).Test().GetType().FullName);
    }
}

public class GenericClass<T1>
{
    public GenericClass(T1 arg)
    {
    }

    public object Test()
    {
        return new GenericClass<SecondGeneric<T1>>(null);
    }
}

public class SecondGeneric<T2>
{
}

public class A
{
}