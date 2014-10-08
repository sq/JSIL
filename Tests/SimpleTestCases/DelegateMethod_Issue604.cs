using System;
using System.Reflection;

public static class Program
{
    public static void Main()
    {
        Test(new Test());
        TestInterface(new Test());

        Action a = M1;
        a += M2;
        WriteAction(a);
    }

    public static void TestInterface(ITest<object> item)
    {
        WriteAction(item.Method1);
        WriteAction(item.Method2);
        //WriteAction(item.GenericMethod1<int>);
        //WriteAction(item.GenericMethod2<string>);

        //WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(ITest<object>).GetMethod("Method1")));
        //WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(ITest<object>).GetMethod("Method2")));
        //WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(ITest<object>).GetMethod("GenericMethod1").MakeGenericMethod(typeof(string))));
        //WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(ITest<object>).GetMethod("GenericMethod2").MakeGenericMethod(typeof(string))));
    }

    public static void Test(Test item)
    {
        WriteAction(item.Method1);
        WriteAction(item.Method2);
        WriteAction(item.GenericMethod1<int>);
        WriteAction(item.GenericMethod2<string>);

        WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(Test).GetMethod("Method1")));
        WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(Test).GetMethod("Method2")));
        WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(Test).GetMethod("GenericMethod1").MakeGenericMethod(typeof(string))));
        WriteAction((Action)Delegate.CreateDelegate(typeof(Action), item, typeof(Test).GetMethod("GenericMethod2").MakeGenericMethod(typeof(string))));
    }

    public static void WriteAction(Action action)
    {
        Console.Write(action.Method.DeclaringType.Name);
        Console.Write(".");
        Console.WriteLine(action.Method.Name);
        Console.WriteLine(action.Method.ContainsGenericParameters ? "true" : "false");

        var mi = (MethodInfo)action.GetType().GetProperty("Method").GetGetMethod().Invoke(action, new object[0]);
        Console.Write(mi.DeclaringType.Name);
        Console.Write(".");
        Console.WriteLine(mi.Name);
        Console.WriteLine(mi.ContainsGenericParameters ? "true" : "false");
    }

    public static void M1()
    {

    }

    public static void M2()
    {

    }
}

public interface ITest<T>
{
    void Method1();

    void Method2();

    void GenericMethod1<Q>();

    void GenericMethod2<Q>();
}

public class Test : ITest<object>
{
    public void Method1() { }

    public void Method2() { }

    public void GenericMethod1<Q>() { Console.WriteLine("Hah!"); }

    public void GenericMethod2<Q>() { }
}