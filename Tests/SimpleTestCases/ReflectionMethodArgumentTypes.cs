using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        Write(typeof(Program).GetMethod("StaticGenericMethod").MakeGenericMethod(typeof(A3), typeof(A4)));
        Write(typeof(Program).GetMethod("StaticNonGenericMethod"));

        Write(typeof(INonGeneric).GetMethod("InstanceGenericMethod1").MakeGenericMethod(typeof(B3), typeof(B4)));
        Write(typeof(INonGeneric).GetMethod("InstanceNonGenericMethod1"));

        Write(typeof(NonGeneric).GetMethod("InstanceGenericMethod1").MakeGenericMethod(typeof(B3), typeof(B4)));
        Write(typeof(NonGeneric).GetMethod("InstanceNonGenericMethod1"));

        Write(typeof(IGeneric<Q>).GetMethod("InstanceGenericMethod2").MakeGenericMethod(typeof(C3), typeof(C4)));
        Write(typeof(IGeneric<Q>).GetMethod("InstanceNonGenericMethod2"));

        Write(typeof(Generic<Q>).GetMethod("InstanceGenericMethod1").MakeGenericMethod(typeof(C3), typeof(C4)));
        Write(typeof(Generic<Q>).GetMethod("InstanceNonGenericMethod1"));
        Write(typeof(Generic<Q>).GetMethod("InstanceGenericMethod2").MakeGenericMethod(typeof(C3), typeof(C4)));
        Write(typeof(Generic<Q>).GetMethod("InstanceNonGenericMethod2"));
    }

    public static void Write(MethodInfo mi)
    {
        Console.WriteLine(mi.ReturnType.Name);
        foreach (var parameterInfo in mi.GetParameters())
        {
            Console.WriteLine(parameterInfo.ParameterType.Name);
        }
    }

    public static T1 StaticGenericMethod<T1, T2>(T2 input, A1 input2)
    {
        return default(T1);
    }

    public static A1 StaticNonGenericMethod(A2 input)
    {
        return null;
    }
}

public interface INonGeneric
{
    T2 InstanceGenericMethod1<T1, T2>(T1 input, B1 input2);
    B2 InstanceNonGenericMethod1(B1 input2);
}

public interface IGeneric<TClass>
{
    T2 InstanceGenericMethod2<T1, T2>(TClass input0, T1 input, C1 input2);
    C2 InstanceNonGenericMethod2(TClass input, C1 input2);
}

public class NonGeneric : INonGeneric
{
    public T2 InstanceGenericMethod1<T1, T2>(T1 input, B1 input2)
    {
        return default(T2);
    }

    public B2 InstanceNonGenericMethod1(B1 input2)
    {
        return null;
    }
}

public class Generic<TClass> : INonGeneric, IGeneric<TClass>
{
    public T2 InstanceGenericMethod1<T1, T2>(T1 input, B1 input2)
    {
        return default(T2);
    }

    public B2 InstanceNonGenericMethod1(B1 input2)
    {
        return null;
    }

    public T2 InstanceGenericMethod2<T1, T2>(TClass input0, T1 input, C1 input2)
    {
        return default(T2);
    }

    public C2 InstanceNonGenericMethod2(TClass input, C1 input2)
    {
        return null;
    }
}

public class Q { }

public class A1 { }
public class A2 { }
public class A3 { }
public class A4 { }

public class B1 { }
public class B2 { }
public class B3 { }
public class B4 { }

public class C1 { }
public class C2 { }
public class C3 { }
public class C4 { }