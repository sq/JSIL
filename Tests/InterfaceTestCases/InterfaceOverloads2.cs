using System;

public interface IInterface
{
    void InterfaceMethod(int x);
    void InterfaceMethod(long x);
    void InterfaceMethod(string s);
}

public class CustomType1 : IInterface
{
    public void InterfaceMethod(int x)
    {
        Console.WriteLine("CustomType1.InterfaceMethod1({0})", x);
    }
    public void InterfaceMethod(long x)
    {
        Console.WriteLine("CustomType1.InterfaceMethod2({0})", x);
    }
    public void InterfaceMethod (string s) 
    {
        Console.WriteLine("CustomType1.InterfaceMethod3({0})", s);
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var a = new CustomType1();
        a.InterfaceMethod(32);
        a.InterfaceMethod("a");

        IInterface b = a;
        b.InterfaceMethod(32);
        b.InterfaceMethod("b");
    }
}