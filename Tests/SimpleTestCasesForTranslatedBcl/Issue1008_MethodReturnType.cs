using System;
using System.Linq.Expressions;

public static class Program
{
    public static void Main()
    {
        Expression<Action> exp = () => Method1<Test>();
        var returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(Test) == returnType ? "true" : "false");

        exp = () => Method1<int>();
        returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(int) == returnType ? "true" : "false");

        exp = () => Method2<Test>();
        returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(Holder<Test>) == returnType ? "true" : "false");

        exp = () => Method2<int>();
        returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(Holder<int>) == returnType ? "true" : "false");

        exp = () => Method3<Test>();
        returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(Test[]) == returnType ? "true" : "false");

        exp = () => Method3<int>();
        returnType = ((MethodCallExpression)exp.Body).Method.ReturnType;
        Console.WriteLine(returnType.GetType().IsSubclassOf(typeof(Type)) ? "true" : "false");
        Console.WriteLine(typeof(int[]) == returnType ? "true" : "false");
    }

    public static T Method1<T>()
    {
        return default(T);
    }

    public static Holder<T> Method2<T>()
    {
        return default(Holder<T>);
    }

    public static T[] Method3<T>()
    {
        return default(T[]);
    }
}

public class Holder<T>
{
}

public class Test
{ }