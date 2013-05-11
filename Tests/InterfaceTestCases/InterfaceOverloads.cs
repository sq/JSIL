using System;

public interface IInterface
{
    void InterfaceMethod(int x);
    void InterfaceMethod(long x);
    int InterfaceProperty { get; set; }
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
    public int InterfaceProperty { get; set; }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var a = new CustomType1();
        a.InterfaceMethod(32);
        IInterface b = a;
        b.InterfaceMethod(32);

        a.InterfaceProperty = 16;
        Console.WriteLine(a.InterfaceProperty);
        Console.WriteLine(b.InterfaceProperty);

        b.InterfaceProperty = 17;
        Console.WriteLine(a.InterfaceProperty);
        Console.WriteLine(b.InterfaceProperty);
    }
}