using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var obj = new Derived();

        Console.WriteLine("**Static**");
        Action<Derived> dDerived = Method;
        Action<Base> dBase = Method;
        Action<IInterface> dIInterface = Method;
        Action<Derived> dGenericDerived = Method<object>;
        Action<Base> dGenericBase = Method<object>;

        dIInterface(obj);
        dBase(obj);
        dDerived(obj);
        dGenericDerived(obj);
        dGenericBase(obj);

        Console.WriteLine("**Non-Virtual**");
        var classObj = new MethodHolder();
        dDerived = classObj.Method;
        dBase = classObj.Method;
        dIInterface = classObj.Method;
        dGenericDerived = classObj.Method<object>;
        dGenericBase = classObj.Method<object>;

        dIInterface(obj);
        dBase(obj);
        dDerived(obj);
        dGenericDerived(obj);
        dGenericBase(obj);

        Console.WriteLine("**Virtual**");
        var vObj = new VirtualMethodHolder();
        dDerived = vObj.Method;
        dBase = vObj.Method;
        dIInterface = vObj.Method;
        dGenericDerived = vObj.Method<object>;
        dGenericBase = vObj.Method<object>;

        dIInterface(obj);
        dBase(obj);
        dDerived(obj);
        dGenericDerived(obj);
        dGenericBase(obj);

        Console.WriteLine("**Interface**");
        var iObj = GetInterfaceHolder();
        dDerived = iObj.Method;
        dBase = iObj.Method;
        dIInterface = iObj.Method;
        dGenericDerived = iObj.Method<object>;
        dGenericBase = iObj.Method<object>;

        dIInterface(obj);
        // Doesn't work due #892
        //dBase(obj);
        //dDerived(obj);
        //dGenericDerived(obj);
        //dGenericBase(obj);
    }

    public static IMethodHolder GetInterfaceHolder()
    {
        return new MethodHolder();
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

    public static void Method<T>(Derived input)
    {
        Console.WriteLine("GenericDerived");
    }

    public static void Method<T>(Base input)
    {
        Console.WriteLine("GenericBase");
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

public interface IMethodHolder
{
    void Method(Base input);
    void Method(IInterface input);
    void Method(Derived input);
    void Method<T>(Derived input);
    void Method<T>(Base input);
}

public class MethodHolder : IMethodHolder
{
    public void Method(Base input)
    {
        Console.WriteLine("Base");
    }

    public void Method(IInterface input)
    {
        Console.WriteLine("IInterface");
    }

    public void Method(Derived input)
    {
        Console.WriteLine("Derived");
    }

    public void Method<T>(Derived input)
    {
        Console.WriteLine("GenericDerived");
    }

    public void Method<T>(Base input)
    {
        Console.WriteLine("GenericBase");
    }
}

public class VirtualMethodHolder : IMethodHolder
{
    public virtual void Method(Base input)
    {
        Console.WriteLine("Base");
    }

    public virtual void Method(IInterface input)
    {
        Console.WriteLine("IInterface");
    }

    public virtual void Method(Derived input)
    {
        Console.WriteLine("Derived");
    }

    public virtual void Method<T>(Derived input)
    {
        Console.WriteLine("GenericDerived");
    }

    public virtual void Method<T>(Base input)
    {
        Console.WriteLine("GenericBase");
    }
}