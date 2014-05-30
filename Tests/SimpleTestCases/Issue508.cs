using System;

public static class Program
{
    public static void Main(string[] args)
    {
		var a = FailingMethod<object>();
        Console.WriteLine(a);
    }

    private static GenericClassWithTwoConstructors<T> FailingMethod<T>()
    {
        Func<GenericClassWithTwoConstructors<T>> f = () => new GenericClassWithTwoConstructors<T>();
	    var result = f();
	    return result;
    }
}

public class GenericClassWithTwoConstructors<T>
{
    public GenericClassWithTwoConstructors(string str)
    {}

    public GenericClassWithTwoConstructors()
    {}
}	