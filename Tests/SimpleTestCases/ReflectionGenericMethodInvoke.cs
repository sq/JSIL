using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Class1.GetGenericMethodInfo() == Class2.GetGenericMethodInfo() ? "true" : "false");

        var mi = typeof(Program).GetMethod("StaticGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(null, new object[] { "TestString4" });
        var actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), null, mi);
        actionStr("TestString5");

        mi = typeof(Program).GetMethod("StaticGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(null, new object[] { 6 });
        var actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), null, mi);
        actionInt(7);

        object obj = new NonGeneric();
        mi = typeof(NonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(obj, new object[] { "TestString1" });
        actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), obj, mi);
        actionStr("TestString2");

        mi = typeof(NonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(obj, new object[] { 3 });
        actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), obj, mi);
        actionInt(4);

        obj = new Generic<int>();
        mi = typeof(Generic<int>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(obj, new object[] { "TestString8" });
        actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), obj, mi);
        actionStr("TestString9");

        mi = typeof(Generic<int>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(obj, new object[] { 10 });
        actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), obj, mi);
        actionInt(11);

        // TODO: Implement interface method reflection invoke
        /*obj = new NonGeneric();
        mi = typeof(INonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(obj, new object[] { "TestString12" });
        actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), obj, mi);
        actionStr("TestString13");

        mi = typeof(INonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(obj, new object[] { 14 });
        actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), obj, mi);
        actionInt(15);

        obj = new Generic<string>();
        mi = typeof(INonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(obj, new object[] { "TestString16" });
        actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), obj, mi);
        actionStr("TestString17");

        mi = typeof(INonGeneric).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(obj, new object[] { 18 });
        actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), obj, mi);
        actionInt(19);

        mi = typeof(IGeneric<string>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
        mi.Invoke(obj, new object[] { "TestString20" });
        actionStr = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), obj, mi);
        actionStr("TestString21");

        mi = typeof(IGeneric<string>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(int) });
        mi.Invoke(obj, new object[] { 22 });
        actionInt = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), obj, mi);
        actionInt(23);*/
    }

    public static void StaticGenericMethod<T>(T input)
    {
        Console.WriteLine(typeof(T).Name);
        Console.WriteLine(input);
    }
}

public interface INonGeneric
{
    void InstanceGenericMethod<T>(T input);
}

public interface IGeneric<TClass>
{
    void InstanceGenericMethod<T>(T input);
}

public class NonGeneric : INonGeneric
{
    private readonly string _field = "field";

    public void InstanceGenericMethod<T>(T input)
    {
        Console.WriteLine(typeof(T).Name);
        Console.WriteLine(input);
        Console.WriteLine(_field);
    }
}

public class Generic<TClass> : INonGeneric, IGeneric<TClass>
{
    private readonly string _field = "field";

    public void InstanceGenericMethod<T>(T input)
    {
        Console.WriteLine(typeof(TClass).Name);
        Console.WriteLine(typeof(T).Name);
        Console.WriteLine(input);
        Console.WriteLine(_field);
    }
}



public static class Class1
{
    public static MethodInfo GetGenericMethodInfo()
    {
        return typeof(Generic<string>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
    }
}

public static class Class2
{
    public static MethodInfo GetGenericMethodInfo()
    {
        return typeof(Generic<string>).GetMethod("InstanceGenericMethod").MakeGenericMethod(new[] { typeof(string) });
    }
}

