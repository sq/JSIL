using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetClass().GenericMethod<int>(1);
        GetClass().GenericMethod<object>(new object());
    }

    public static IInterfaceWithGeneric GetClass()
    {
        return new ClassWithGeneric();
    }
}

public interface IInterfaceWithGeneric
{
    void GenericMethod<T>(object item);
}

public class ClassWithGeneric : IInterfaceWithGeneric
{
    public void GenericMethod<T>(object item)
    {
        Console.WriteLine(typeof(T));
    }
}