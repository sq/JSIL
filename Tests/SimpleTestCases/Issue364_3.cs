using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetImplementor().Method((Interface1)new Type());
        GetImplementor().Method((Interface2)new Type());
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

public interface Interface1
{ }

public interface Interface2
{ }

public class Type : Interface1, Interface2
{
}

public interface IInterface
{
    void Method(Interface1 item);
    void Method(Interface2 item);
}

public class Implementor : IInterface
{
    public void Method(Interface1 item)
    {
        Console.WriteLine("Interface1");
    }

    public void Method(Interface2 item)
    {
        Console.WriteLine("Interface2");
    }
}