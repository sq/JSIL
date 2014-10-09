using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetClass().GenericMethod(1);
        GetClass().GenericMethod(new object());
    }

    public static IInterfaceWithGeneric GetClass()
    {
        return new ClassWithGeneric();
    }
}

public interface IInterfaceWithGeneric
{
    void GenericMethod<T>(T item);
}

public class ClassWithGeneric : IInterfaceWithGeneric
{
    public void GenericMethod<T>(T item)
    {
        Console.WriteLine(typeof(T));
    }
}