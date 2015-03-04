using System;
using System.Runtime.InteropServices;

public static class Program
{
    public static void Main()
    {
        var d =
            (Action<NonGenericClass, string>)
                Delegate.CreateDelegate(typeof(Action<NonGenericClass, string>), null,
                    typeof(NonGenericClass).GetMethod("Run"));
        d(new NonGenericClass(1), "in");
        d(null, "in");

        var d2 =
            (Action<NonGenericClass, string>)
                Delegate.CreateDelegate(typeof(Action<NonGenericClass, string>), null,
                    typeof(NonGenericClass).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d2(new NonGenericClass(1), "in");
        d2(null, "in");

        var d3 =
            (Action<GenericClass<string>, string>)
                Delegate.CreateDelegate(typeof(Action<GenericClass<string>, string>), null,
                    typeof(GenericClass<string>).GetMethod("Run"));
        d3(new GenericClass<string>(1), "in");
        d3(null, "in");

        var d4 =
            (Action<GenericClass<string>, string, string>)
                Delegate.CreateDelegate(typeof(Action<GenericClass<string>, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d4(new GenericClass<string>(1), "in", "in2");
        d4(null, "in", "in2");

        var d5 =
            (Func<NonGenericClass, string, string>)
                Delegate.CreateDelegate(typeof(Func<NonGenericClass, string, string>), null,
                    typeof(NonGenericClass).GetMethod("RunOutput"));
        Console.WriteLine(d5(new NonGenericClass(1), "in"));
        Console.WriteLine(d5(null, "in"));

        var d6 =
            (Func<NonGenericClass, string, string>)
                Delegate.CreateDelegate(typeof(Func<NonGenericClass, string, string>), null,
                    typeof(NonGenericClass).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d6(new NonGenericClass(1), "in"));
        Console.WriteLine(d6(null, "in"));

        var d7 =
            (Func<GenericClass<string>, string, string>)
                Delegate.CreateDelegate(typeof(Func<GenericClass<string>, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunOutput"));
        Console.WriteLine(d7(new GenericClass<string>(1), "in"));
        Console.WriteLine(d7(null, "in"));

        var d8 =
            (Func<GenericClass<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<GenericClass<string>, string, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d8(new GenericClass<string>(1), "in", "in2"));
        Console.WriteLine(d8(null, "in", "in2"));

        var d9 =
    (Action<IInterface, string>)
        Delegate.CreateDelegate(typeof(Action<IInterface, string>), null,
            typeof(IInterface).GetMethod("Run"));
        d9(new NonGenericClassImplementor(1), "in");
        d9(new GenericClassImplementor<string>(2), "in");

        //Looks like .Net doesn't support unbinded delegate for generic interface methods.
        /*var d10 =
    (Action<IInterface, string>)
        Delegate.CreateDelegate(typeof(Action<IInterface, string>), null,
            typeof(IInterface).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d10(new NonGenericClassImplementor(1), "in");
        d10(new GenericClassImplementor<string>(2), "in");*/


        var d11 =
    (Action<IGenericInterface<string>, string>)
        Delegate.CreateDelegate(typeof(Action<IGenericInterface<string>, string>), null,
            typeof(IGenericInterface<string>).GetMethod("Run"));
        d11(new NonGenericClassImplementor(1), "in");
        d11(new GenericClassImplementor<string>(2), "in");

        /*var d12 =
            (Action<IGenericInterface<string>, string, string>)
                Delegate.CreateDelegate(typeof(Action<IGenericInterface<string>, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunGeneric2").MakeGenericMethod(typeof(string)));
        d12(new NonGenericClassImplementor(1), "in", "in2");
        d12(new GenericClassImplementor<string>(2), "in", "in2");*/

        var d12 =
    (Func<IInterface, string, string>)
        Delegate.CreateDelegate(typeof(Func<IInterface, string, string>), null,
            typeof(IInterface).GetMethod("RunOutput"));
        Console.WriteLine(d12(new NonGenericClassImplementor(1), "in"));
        Console.WriteLine(d12(new GenericClassImplementor<string>(2), "in"));

        /*var d13 =
            (Func<IInterface, string, string>)
                Delegate.CreateDelegate(typeof(Func<IInterface, string, string>), null,
                    typeof(IInterface).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d13(new NonGenericClassImplementor(1), "in"));
        Console.WriteLine(d13(new GenericClassImplementor<string>(2), "in"));*/

        var d14 =
            (Func<IGenericInterface<string>, string, string>)
                Delegate.CreateDelegate(typeof(Func<IGenericInterface<string>, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunOutput"));
        Console.WriteLine(d14(new NonGenericClassImplementor(1), "in"));
        Console.WriteLine(d14(new GenericClassImplementor<string>(2), "in"));

        /*var d15 =
            (Func<IGenericInterface<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<IGenericInterface<string>, string, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunOutputGeneric2").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d15(new NonGenericClassImplementor(1), "in", "in2"));
        Console.WriteLine(d15(new GenericClassImplementor<string>(2), "in", "in2"));*/
    }
}


public class NonGenericClass
{
    public int _value;

    public NonGenericClass(int value)
    {
        _value = value;
    }

    public void Run(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this==null; Input: " + input);
        }
    }

    public void RunGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input);
        }
    }

    public string RunOutput(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this==null; Input: " + input);
        }

        return "output1";
    }

    public string RunOutputGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input);
        }

        return "output2";
    }
}

public class GenericClass<T>
{
    public int _value;

    public GenericClass(int value)
    {
        _value = value;
    }

    public void Run(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Non-Generic, this==null; Input: " + input);
        }
    }

    public void RunGeneric<T2>(T input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }
    }

    public string RunOutput(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Non-Generic, this==null; Input: " + input);
        }

        return "output3";
    }

    public string RunOutputGeneric<T2>(T input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }

        return "output4";
    }
}

public interface IInterface
{
    void Run(string input);
    void RunGeneric<T>(T input);

    string RunOutput(string input);
    string RunOutputGeneric<T>(T input);
}

public interface IGenericInterface<T>
{
    void Run(string input);
    void RunGeneric2<T2>(T input, T2 input2);

    string RunOutput(string input);
    string RunOutputGeneric2<T2>(T input, T2 input2);
}

public class NonGenericClassImplementor : IInterface, IGenericInterface<string>
{
    public int _value;

    public NonGenericClassImplementor(int value)
    {
        _value = value;
    }

    public void Run(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this==null; Input: " + input);
        }
    }

    public void RunGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input);
        }
    }

    public void RunGeneric2<T2>(string input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }
    }

    public string RunOutput(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Non-Generic, this==null; Input: " + input);
        }

        return "99";
    }

    public string RunOutputGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input);
        }

        return "100";
    }

    public string RunOutputGeneric2<T2>(string input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Non-Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Non-Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }

        return "101";
    }
}

public class GenericClassImplementor<T> : IGenericInterface<T>, IInterface
{
    public int _value;

    public GenericClassImplementor(int value)
    {
        _value = value;
    }

    public void Run(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Non-Generic, this==null; Input: " + input);
        }
    }

    public void RunGeneric2<T2>(T input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }
    }

    public void RunGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input);
        }
    }

    public string RunOutput(string input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Non-Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Non-Generic, this==null; Input: " + input);
        }

        return "110";
    }

    public string RunOutputGeneric2<T2>(T input, T2 input2)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input + "; Input2: " + input2);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input + "; Input2: " + input2);
        }

        return "111";
    }

    public string RunOutputGeneric<T>(T input)
    {
        if (this != null)
        {
            Console.WriteLine("Generic class, Generic, this: " + _value + "; Input: " + input);
        }
        else
        {
            Console.WriteLine("Generic class, Generic, this==null " + input);
        }

        return "112";
    }
}