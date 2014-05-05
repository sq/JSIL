using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var derived = new Derived();
        Console.WriteLine(derived.Method().GetType().FullName);
    }
}

public class Base
{
    public BaseResult Method()
    {
        return new BaseResult();
    }
}

public class Derived : Base
{
    public new DerivedResult Method()
    {
        return new DerivedResult();
    }
}

public class BaseResult
{
}

public class DerivedResult
{
}