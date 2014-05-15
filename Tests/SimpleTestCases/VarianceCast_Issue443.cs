using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        CheckCovariant();
        CheckContravariant();
    }

    public static void CheckCovariant()
    {
        var a = (object)new CovarinatImplementor<BaseClass>();
        try
        {
            var b = (ICovarinatInterface<DerivedClass>) a;
            b.Get();
        }
        catch (Exception)
        {
            Console.WriteLine("Covarint downcast expected exception");
        }
        {
            var b = (ICovarinatInterface<BaseClass>)a;
            b.Get();
            Console.WriteLine("Covarint interface cast successful");
        }
        {
            var b = (ICovarinatInterface<object>) a;
            b.Get();
            Console.WriteLine("Covarint upast successful");
        }
    }

    public static void CheckContravariant()
    {
        var a = (object)new ContravarinatImplementor<BaseClass>();
        try
        {
            var b = (IContravarinatInterface<object>) a;
            b.Set(new object());
        }
        catch (Exception)
        {
            Console.WriteLine("Contravarinat upcast expected exception");
        }

        {
            var b = (IContravarinatInterface<BaseClass>)a;
            b.Set(new DerivedClass());
            Console.WriteLine("Contravarinat interface cast successful");
        }
        {
            var b = (IContravarinatInterface<DerivedClass>) a;
            b.Set(new DerivedClass());
            Console.WriteLine("Contravarinat downcat successful");
        }
    }
}

public interface ICovarinatInterface<out T>
{
    T Get();
}

public class CovarinatImplementor<T> : ICovarinatInterface<T>
{
    public T Get()
    {
        Console.WriteLine("Get:" + typeof(T).Name);
        return default(T);
    }
}

public interface IContravarinatInterface<in T>
{
    void Set(T input);
}

public class ContravarinatImplementor<T> : IContravarinatInterface<T>
{
    public void Set(T input)
    {
        Console.WriteLine("Set+" + typeof(T).Name + ", " + input.GetType().Name);
    }
}

public class BaseClass {}
public class DerivedClass : BaseClass {}