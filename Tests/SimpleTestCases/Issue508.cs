using System;

public static class Program
{
    public static void Main(string[] args)
    {
    }

    private static void FailingMethod<T>()
    {
        Func<GenericClassWithTwoConstructors<T>> f = () => new GenericClassWithTwoConstructors<T>();
    }
}

public class GenericClassWithTwoConstructors<T>
{
    public GenericClassWithTwoConstructors(string str)
    {}

    public GenericClassWithTwoConstructors()
    {}
}