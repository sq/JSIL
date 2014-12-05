using System;

public static class Program
{
    public static void Main(string[] args)
    {
        GenericClass1<A>.Test();
    }
}

public class GenericClass1<T1>
{
    public static void Test()
    {
        GenericClass1<GenericClass1<T1>>.TestMethod((GenericClass1<T1>)null);
    }

    public static void TestMethod(T1 arg)
    {
        Console.WriteLine("1: " + typeof(T1).FullName);
    }

    public static void TestMethod(string a)
    {

    }

}

public class A
{
}