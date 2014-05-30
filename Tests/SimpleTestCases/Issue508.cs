using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = FailingMethod<TestType>();
        Console.WriteLine(a);
    }

    private static GenericClassWithTwoConstructors<T> FailingMethod<T>()
    {
        Func<GenericClassWithTwoConstructors<T>> f = () => new GenericClassWithTwoConstructors<T>();
	    var result = f();
	    return result;
    }
}

public class TestType
{}

public class GenericClassWithTwoConstructors<T>
{
    public GenericClassWithTwoConstructors( string str )
    {
    	
    }
    
    public GenericClassWithTwoConstructors()
    {}
}	