using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GetImplementor().Method(new FirstType(), () => null);
    }

    private static IInterface GetImplementor()
    {
        return new Implementor();
    }
}

public interface IInterface
{
    void Method(FirstType item, Func<object> action);
}

public class Implementor : IInterface
{
    public void Method(FirstType item, Func<object> action)
    {
    }

    public void Method(SecondType item, Func<object> action)
    {
    }
}

public class FirstType
{
}

public class SecondType
{
}