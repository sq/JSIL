using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var obj = new CustomType();
        Run(obj);
        new GenericTypeWithConstraint<CustomType>().Run(obj);
    }

    public static void Run<T>(T item) where T : IIterfaceForGenericMethodTest
    {
        item.MethodFromIIterfaceForGenericMethodTest();
    }
}

public class GenericTypeWithConstraint<T>
    where T : IIterfaceForGenericClassTest
{
    public void Run(T item)
    {
        item.MethodFromIIterfaceForGenericClassTest();
    }
}

public interface IIterfaceForGenericMethodTest
{
    void MethodFromIIterfaceForGenericMethodTest();
}

public interface IIterfaceForGenericClassTest
{
    void MethodFromIIterfaceForGenericClassTest();
}


public class CustomType : IIterfaceForGenericMethodTest, IIterfaceForGenericClassTest
{
    public void MethodFromIIterfaceForGenericMethodTest()
    {
        Console.WriteLine("CustomType.IIterfaceForGenericMethodTest - used");
    }

    public void MethodFromIIterfaceForGenericClassTest()
    {
        Console.WriteLine("CustomType.MethodFromIIterfaceForGenericClassTest - used");
    }
}