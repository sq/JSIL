using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetImplementor().GetResult();
        GetImplementor2().GetResult();
    }

    public static IInterface<Base> GetImplementor()
    {
        return new Implementor<Derived>();
    }

    public static IInterface<Base> GetImplementor2()
    {
        return new Implementor<Derived2>();
    }
}

public interface IInterface<out T>
{
    T GetResult();
}

public class Implementor<T> : IInterface<T>
{
    public T GetResult()
    {
        Console.WriteLine(typeof(T).FullName);
        return default(T);
    }
}

public class Base
{
}

public class Derived : Base
{
}

public class Derived2 : Base
{
}