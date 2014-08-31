using System;
using System.Linq.Expressions;
using System.Reflection;

public static class Program
{
    public static A Field;

    public static void Main(string[] args)
    {
        WriteMethod(() => StaticMethodNonGeneric());
        WriteMethod(() => StaticMethodGeneric<A>());

        WriteMethod((NonGenericClass nonGenericClass) => nonGenericClass.InstanceNonGenericMethod());
        WriteMethod((NonGenericClass nonGenericClass) => nonGenericClass.InstanceGenericMethod<A>());

        WriteMethod((INonGenericInterface interfaceInstance) => interfaceInstance.InstanceNonGenericMethod());
        WriteMethod((INonGenericInterface interfaceInstance) => interfaceInstance.InstanceGenericMethod<A>());

        WriteMethod((GenericClass<B> genericClass) => genericClass.InstanceNonGenericMethod());
        WriteMethod((GenericClass<B> genericClass) => genericClass.InstanceGenericMethod<A>());

        WriteMethod((IGenericInterface<B> interfaceGenericInstance) => interfaceGenericInstance.InstanceNonGenericMethod());
        WriteMethod((IGenericInterface<B> interfaceGenericInstance) => interfaceGenericInstance.InstanceGenericMethod<A>());

        WriteField(() => Field);
        WriteField((NonGenericClass nonGenericClass) => nonGenericClass.FieldOfNonGenericClass);
        WriteField((GenericClass<B> genericClass) => genericClass.FieldOfNonGenericClass1);
        WriteField((GenericClass<B> genericClass) => genericClass.FieldOfNonGenericClass2);
    }

    public static void StaticMethodNonGeneric()
    { }

    public static T StaticMethodGeneric<T>()
    {
        return default(T);
    }

    private static void WriteMethod(Expression<Action> expression)
    {
        WriteMethodInternal(expression);
    }

    private static void WriteMethod<T>(Expression<Action<T>> expression)
    {
        WriteMethodInternal(expression);
    }

    private static void WriteMethodInternal(LambdaExpression expression)
    {
        var mi = ExtractMethodInfo(expression);
        Console.WriteLine("{2} {0}.{1}(...)", mi.DeclaringType.Name, mi.Name, mi.ReturnType.Name);
    }

    private static void WriteField<TField>(Expression<Func<TField>> expression)
    {
        WriteFieldInternal(expression);
    }

    private static void WriteField<TInput, TField>(Expression<Func<TInput, TField>> expression)
    {
        WriteFieldInternal(expression);
    }

    private static void WriteFieldInternal(LambdaExpression expression)
    {
        var mi = ExtractFieldInfo(expression);
        Console.WriteLine("{2} {0}.{1}", mi.DeclaringType.Name, mi.Name, mi.FieldType.Name);
    }

    private static MethodInfo ExtractMethodInfo(LambdaExpression expression)
    {
        return ((MethodCallExpression)(expression.Body)).Method;
    }

    private static FieldInfo ExtractFieldInfo(LambdaExpression expression)
    {
        return (FieldInfo)((MemberExpression)(expression.Body)).Member;
    }
}

public interface INonGenericInterface
{
    void InstanceNonGenericMethod();
    T InstanceGenericMethod<T>();
}

public class NonGenericClass : INonGenericInterface
{
    public A FieldOfNonGenericClass;

    public void InstanceNonGenericMethod()
    {
    }

    public T InstanceGenericMethod<T>()
    {
        return default(T);
    }
}

public interface IGenericInterface<T>
{
    T InstanceNonGenericMethod();
    T2 InstanceGenericMethod<T2>();
}

public class GenericClass<T> : IGenericInterface<T>
{
    public A FieldOfNonGenericClass1;
    public T FieldOfNonGenericClass2;

    public T InstanceNonGenericMethod()
    {
        return default(T);
    }

    public T2 InstanceGenericMethod<T2>()
    {
        return default(T2);
    }
}

public class A { }
public class B { }