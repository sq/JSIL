using System;
using System.Runtime.InteropServices;

public static class Program
{
    public static void Main()
    {
        var s =
            (Action<string, string>)
                Delegate.CreateDelegate(typeof(Action<string, string>), null,
                    typeof(StaticClass).GetMethod("Run"));
        s("in", "in99");

        var s2 =
    (Action<string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string>), null,
            typeof(StaticClass).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        s2("in", "in99");

        var s3 =
    (Action<string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string>), null,
            typeof(StaticGenericClass<string>).GetMethod("Run"));
        s3("in", "in99");

        var s4 =
(Action<string, string, string>)
Delegate.CreateDelegate(typeof(Action<string, string, string>), null,
    typeof(StaticGenericClass<string>).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        s4("in", "in2", "in99");

        var s5 =
(Func<string, string, string>)
Delegate.CreateDelegate(typeof(Func<string, string, string>), null,
    typeof(StaticClass).GetMethod("RunOutput"));
        Console.WriteLine(s5("in", "in99"));

        var s6 =
(Func<string, string, string>)
Delegate.CreateDelegate(typeof(Func<string, string, string>), null,
    typeof(StaticClass).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(s6("in", "in99"));

        var s7 =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), null,
            typeof(StaticGenericClass<string>).GetMethod("RunOutput"));
        Console.WriteLine(s7("in", "in99"));

        var s8 =
    (Func<string, string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string, string>), null,
            typeof(StaticGenericClass<string>).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(s8("in", "in2", "in99"));

        var d =
            (Action<NonGenericClass, string, string>)
                Delegate.CreateDelegate(typeof(Action<NonGenericClass, string, string>), null,
                    typeof(NonGenericClass).GetMethod("Run"));
        d(new NonGenericClass(1), "in", "in99");
        d(null, "in", "in99");

        var d_ =
            (Action<string, string>)
                Delegate.CreateDelegate(typeof(Action<string, string>), new NonGenericClass(1),
                    typeof(NonGenericClass).GetMethod("Run"));
        d_("in", "in99");

        var d2 =
            (Action<NonGenericClass, string, string>)
                Delegate.CreateDelegate(typeof(Action<NonGenericClass, string, string>), null,
                    typeof(NonGenericClass).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d2(new NonGenericClass(1), "in", "in99");
        d2(null, "in", "in99");

        var d2_ =
    (Action<string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string>), new NonGenericClass(1),
            typeof(NonGenericClass).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d2_("in", "in99");

        var d3 =
            (Action<GenericClass<string>, string, string>)
                Delegate.CreateDelegate(typeof(Action<GenericClass<string>, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("Run"));
        d3(new GenericClass<string>(1), "in", "in99");
        d3(null, "in", "in99");

        var d3_ =
    (Action<string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string>), new GenericClass<string>(1),
            typeof(GenericClass<string>).GetMethod("Run"));
        d3_("in", "in99");

        var d4 =
            (Action<GenericClass<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Action<GenericClass<string>, string, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d4(new GenericClass<string>(1), "in", "in2", "in99");
        d4(null, "in", "in2", "in99");

        var d4_ =
    (Action<string, string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string, string>), new GenericClass<string>(1),
            typeof(GenericClass<string>).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d4_("in", "in2", "in99");

        var d5 =
            (Func<NonGenericClass, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<NonGenericClass, string, string, string>), null,
                    typeof(NonGenericClass).GetMethod("RunOutput"));
        Console.WriteLine(d5(new NonGenericClass(1), "in", "in99"));
        Console.WriteLine(d5(null, "in", "in99"));

        var d5_ =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), new NonGenericClass(1),
            typeof(NonGenericClass).GetMethod("RunOutput"));
        Console.WriteLine(d5_("in", "in99"));

        var d6 =
            (Func<NonGenericClass, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<NonGenericClass, string, string, string>), null,
                    typeof(NonGenericClass).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d6(new NonGenericClass(1), "in", "in99"));
        Console.WriteLine(d6(null, "in", "in99"));

        var d6_ =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), new NonGenericClass(1),
            typeof(NonGenericClass).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d6_("in", "in99"));

        var d7 =
            (Func<GenericClass<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<GenericClass<string>, string, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunOutput"));
        Console.WriteLine(d7(new GenericClass<string>(1), "in", "in99"));
        Console.WriteLine(d7(null, "in", "in99"));

        var d7_ =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), new GenericClass<string>(1),
            typeof(GenericClass<string>).GetMethod("RunOutput"));
        Console.WriteLine(d7_("in", "in99"));

        var d8 =
            (Func<GenericClass<string>, string, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<GenericClass<string>, string, string, string, string>), null,
                    typeof(GenericClass<string>).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d8(new GenericClass<string>(1), "in", "in2", "in99"));
        Console.WriteLine(d8(null, "in", "in2", "in99"));

        var d8_ =
    (Func<string, string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string, string>), new GenericClass<string>(1),
            typeof(GenericClass<string>).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d8_("in", "in2", "in99"));

        var d9 =
    (Action<IInterface, string, string>)
        Delegate.CreateDelegate(typeof(Action<IInterface, string, string>), null,
            typeof(IInterface).GetMethod("Run"));
        d9(new NonGenericClassImplementor(1), "in", "in99");
        d9(new GenericClassImplementor<string>(2), "in", "in99");

        var d9_ =
(Action<string, string>)
Delegate.CreateDelegate(typeof(Action<string, string>), new NonGenericClassImplementor(1),
    typeof(IInterface).GetMethod("Run"));
        d9_("in", "in99");

        var d9_2 =
(Action<string, string>)
Delegate.CreateDelegate(typeof(Action<string, string>), new GenericClassImplementor<string>(2),
typeof(IInterface).GetMethod("Run"));
        d9_2("in", "in99");

        //Looks like .Net doesn't support unbinded delegate for generic interface methods.
        /*var d10 =
    (Action<IInterface, string, string>)
        Delegate.CreateDelegate(typeof(Action<IInterface, string, string>), null,
            typeof(IInterface).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d10(new NonGenericClassImplementor(1), "in", "in99");
        d10(new GenericClassImplementor<string>(2), "in", "in99");*/

        var d10_ =
    (Action<string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string>), new NonGenericClassImplementor(1),
            typeof(IInterface).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d10_("in", "in99");
        var d10_2 =
(Action<string, string>)
Delegate.CreateDelegate(typeof(Action<string, string>), new GenericClassImplementor<string>(2),
    typeof(IInterface).GetMethod("RunGeneric").MakeGenericMethod(typeof(string)));
        d10_2("in", "in99");

        var d11 =
    (Action<IGenericInterface<string>, string, string>)
        Delegate.CreateDelegate(typeof(Action<IGenericInterface<string>, string, string>), null,
            typeof(IGenericInterface<string>).GetMethod("Run"));
        d11(new NonGenericClassImplementor(1), "in", "in99");
        d11(new GenericClassImplementor<string>(2), "in", "in99");

        var d11_ =
(Action<string, string>)
Delegate.CreateDelegate(typeof(Action<string, string>), new NonGenericClassImplementor(1),
    typeof(IGenericInterface<string>).GetMethod("Run"));
        d11_("in", "in99");
        var d11_2 =
(Action<string, string>)
Delegate.CreateDelegate(typeof(Action<string, string>), new GenericClassImplementor<string>(2),
typeof(IGenericInterface<string>).GetMethod("Run"));
        d11_2("in", "in99");

        /*var d12 =
            (Action<IGenericInterface<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Action<IGenericInterface<string>, string, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunGeneric2").MakeGenericMethod(typeof(string)));
        d12(new NonGenericClassImplementor(1), "in", "in2", "in99");
        d12(new GenericClassImplementor<string>(2), "in", "in2", "in99");*/

        var d12_ =
            (Action<string, string, string>)
                Delegate.CreateDelegate(typeof(Action<string, string, string>), new NonGenericClassImplementor(1),
                    typeof(IGenericInterface<string>).GetMethod("RunGeneric2").MakeGenericMethod(typeof(string)));
        d12_("in", "in2", "in99");
        var d12_2 =
    (Action<string, string, string>)
        Delegate.CreateDelegate(typeof(Action<string, string, string>), new GenericClassImplementor<string>(2),
            typeof(IGenericInterface<string>).GetMethod("RunGeneric2").MakeGenericMethod(typeof(string)));
        d12_2("in", "in2", "in99");

        var d13 =
    (Func<IInterface, string, string, string>)
        Delegate.CreateDelegate(typeof(Func<IInterface, string, string, string>), null,
            typeof(IInterface).GetMethod("RunOutput"));
        Console.WriteLine(d13(new NonGenericClassImplementor(1), "in", "in99"));
        Console.WriteLine(d13(new GenericClassImplementor<string>(2), "in", "in99"));

        var d13_ =
(Func<string, string, string>)
Delegate.CreateDelegate(typeof(Func<string, string, string>), new NonGenericClassImplementor(1),
    typeof(IInterface).GetMethod("RunOutput"));
        Console.WriteLine(d13_("in", "in99"));
        var d13_2 =
(Func<string, string, string>)
Delegate.CreateDelegate(typeof(Func<string, string, string>), new GenericClassImplementor<string>(2),
typeof(IInterface).GetMethod("RunOutput"));
        Console.WriteLine(d13_2("in", "in99"));

        /*var d14 =
            (Func<IInterface, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<IInterface, string, string, string>), null,
                    typeof(IInterface).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d14(new NonGenericClassImplementor(1), "in", "in99"));
        Console.WriteLine(d14(new GenericClassImplementor<string>(2), "in", "in99"));*/

        var d14_ =
            (Func<string, string, string>)
                Delegate.CreateDelegate(typeof(Func<string, string, string>), new NonGenericClassImplementor(1),
                    typeof(IInterface).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d14_("in", "in99"));
        var d14_2 =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), new GenericClassImplementor<string>(1),
            typeof(IInterface).GetMethod("RunOutputGeneric").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d14_2("in", "in99"));

        var d15 =
            (Func<IGenericInterface<string>, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<IGenericInterface<string>, string, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunOutput"));
        Console.WriteLine(d15(new NonGenericClassImplementor(1), "in", "in99"));
        Console.WriteLine(d15(new GenericClassImplementor<string>(2), "in", "in99"));

        var d15_ =
    (Func<string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string>), new NonGenericClassImplementor(1),
            typeof(IGenericInterface<string>).GetMethod("RunOutput"));
        Console.WriteLine(d15_("in", "in99"));
        var d15_2 =
(Func<string, string, string>)
Delegate.CreateDelegate(typeof(Func<string, string, string>), new GenericClassImplementor<string>(2),
    typeof(IGenericInterface<string>).GetMethod("RunOutput"));
        Console.WriteLine(d15_2("in", "in99"));

        /*var d16 =
            (Func<IGenericInterface<string>, string, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<IGenericInterface<string>, string, string, string, string>), null,
                    typeof(IGenericInterface<string>).GetMethod("RunOutputGeneric2").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d16(new NonGenericClassImplementor(1), "in", "in2", "in99"));
        Console.WriteLine(d16(new GenericClassImplementor<string>(2), "in", "in2", "in99"));*/
        var d16_ =
            (Func<string, string, string, string>)
                Delegate.CreateDelegate(typeof(Func<string, string, string, string>), new NonGenericClassImplementor(1),
                    typeof(IGenericInterface<string>).GetMethod("RunOutputGeneric2").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d16_("in", "in2", "in99"));
        var d16_2 =
    (Func<string, string, string, string>)
        Delegate.CreateDelegate(typeof(Func<string, string, string, string>), new GenericClassImplementor<string>(2),
            typeof(IGenericInterface<string>).GetMethod("RunOutputGeneric2").MakeGenericMethod(typeof(string)));
        Console.WriteLine(d16_2("in", "in2", "in99"));
    }
}

public static class StaticClass
{
    public static void Run(string input, string input2)
    {
        Console.WriteLine("Static class, Non-Generic; Input: " + input);
    }

    public static void RunGeneric<T>(T input, string input2)
    {
        Console.WriteLine("Static class, Non-Generic; Input:  " + input);
    }

    public static string RunOutput(string input, string input2)
    {
        Console.WriteLine("Static class, Non-Generic; Input:  " + input);

        return "output1";
    }

    public static string RunOutputGeneric<T>(T input, string input2)
    {
        Console.WriteLine("Static class, Non-Generic; Input:  " + input);

        return "output2";
    }
}


public class NonGenericClass
{
    public int _value;

    public NonGenericClass(int value)
    {
        _value = value;
    }

    public void Run(string input, string input2)
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

    public void RunGeneric<T>(T input, string input2)
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

    public string RunOutput(string input, string input2)
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

    public string RunOutputGeneric<T>(T input, string input2)
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

public static class StaticGenericClass<T>
{
    public static void Run(string input, string input2)
    {
        Console.WriteLine(
            "Static generic class, Non-Generic; Input: " + input);
    }

    public static void RunGeneric<T2>(T input, T2 input2, string input3)
    {
        Console.WriteLine("Static generic class, Generic; Input: " + input + "; Input2: " + input2);
    }

    public static string RunOutput(string input, string input2)
    {
        Console.WriteLine("Static generic class, Non-Generic; Input: " + input);
        return "output3";
    }

    public static string RunOutputGeneric<T2>(T input, T2 input2, string input3)
    {
        Console.WriteLine("Static generic class, Generic; Input: " + input + "; Input2: " + input2);
        return "output4";
    }
}

public class GenericClass<T>
{
    public int _value;

    public GenericClass(int value)
    {
        _value = value;
    }

    public void Run(string input, string input2)
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

    public void RunGeneric<T2>(T input, T2 input2, string input3)
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

    public string RunOutput(string input, string input2)
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

    public string RunOutputGeneric<T2>(T input, T2 input2, string input3)
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
    void Run(string input, string input2);
    void RunGeneric<T>(T input, string input2);

    string RunOutput(string input, string input2);
    string RunOutputGeneric<T>(T input, string input2);
}

public interface IGenericInterface<T>
{
    void Run(string input, string input2);
    void RunGeneric2<T2>(T input, T2 input2, string input3);

    string RunOutput(string input, string input2);
    string RunOutputGeneric2<T2>(T input, T2 input2, string input3);
}

public class NonGenericClassImplementor : IInterface, IGenericInterface<string>
{
    public int _value;

    public NonGenericClassImplementor(int value)
    {
        _value = value;
    }

    public void Run(string input, string input2)
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

    public void RunGeneric<T>(T input, string input2)
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

    public void RunGeneric2<T2>(string input, T2 input2, string input3)
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

    public string RunOutput(string input, string input2)
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

    public string RunOutputGeneric<T>(T input, string input2)
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

    public string RunOutputGeneric2<T2>(string input, T2 input2, string input3)
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

    public void Run(string input, string input2)
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

    public void RunGeneric2<T2>(T input, T2 input2, string input3)
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

    public void RunGeneric<T>(T input, string input2)
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

    public string RunOutput(string input, string input2)
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

    public string RunOutputGeneric2<T2>(T input, T2 input2, string input3)
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

    public string RunOutputGeneric<T>(T input, string input3)
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