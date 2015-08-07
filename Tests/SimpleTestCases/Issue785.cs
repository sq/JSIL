using System;

public interface Interface1<T>
{
}

public class GenericClass<T> : Interface1<T>
{
    public bool Test(Interface1<T> input)
    {
        return this == input;
    }
}

public class GenericClass2<T> : Interface1<T>
{
}


public static class Program
{
    public static void Main(string[] args)
    {
        var instance = new GenericClass<object>();
        Console.WriteLine(instance.Test(instance) ? "True" : "False");
        Console.WriteLine(instance.Test(new GenericClass2<object>()) ? "True" : "False");
    }
}