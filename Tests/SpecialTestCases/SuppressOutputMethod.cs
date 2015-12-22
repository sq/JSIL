using System;
using JSIL.Meta;

public static class Program
{
    public static void Main(string[] args)
    {
        var instance = new Test();
        try
        {
            instance.Foo();
        }
        catch (Exception e)
        {
            Console.WriteLine("Excpetion in instance.Foo()");
        }

    }
}

public class Test {
    [JSSuppressOutput]
    public void Foo () {
        Console.WriteLine("Foo");
    }
}