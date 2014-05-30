using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = GetGenericList<object>();
        Console.WriteLine(a);
    }

    public static List<T> GetGenericList<T>()
    {
        Func<List<T>> listCreator = () => new List<T>();
	    var result = listCreator();
		return result;
    }
}