using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetImplementor().Method((Type1)null);
        GetImplementor().Method((Type2)null);
    }

    public static object Method()
    {
        return null;
    }

    private static IInterface GetImplementor()
    {
        return new Implementor();
    }
}

public class Type1
{
}

public class Type2
{
}

public interface IInterface
{
    void Method(Type1 item);
    void Method(Type2 item);
}

public class Implementor : IInterface
{
    public void Method(Type1 item)
    {
        Console.WriteLine("Type1");
    }

    public void Method(Type2 item)
    {
        Console.WriteLine("Type2");
    }
}