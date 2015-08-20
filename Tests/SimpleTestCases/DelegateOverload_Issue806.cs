using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Action<Derived> dDerived = Method;
        Action<Base> dBase = Method;
        Action<IInterface> dIInterface = Method;

        var obj = new Derived();
        dIInterface(obj);
        dBase(obj);
        dDerived(obj);
    }

    public static void Method(Base input)
    {
        Console.WriteLine("Base");
    }

    public static void Method(IInterface input)
    {
        Console.WriteLine("IInterface");
    }

    public static void Method(Derived input)
    {
        Console.WriteLine("Derived");
    }
}


public class Base : IInterface
{
}

public class Derived : Base
{
}

public interface IInterface
{
}