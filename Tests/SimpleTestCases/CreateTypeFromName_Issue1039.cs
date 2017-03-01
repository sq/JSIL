using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var dictionary = new Dictionary<A,B>();
        var dictionaryType = dictionary.GetType();
        var type = Type.GetType(dictionaryType.FullName);
        if (type != null)
            Activator.CreateInstance(type);
        Console.WriteLine(
            "{0},{1}", dictionaryType.Name,dictionaryType.FullName
            );
    }
}

public class A { }
public class B { }