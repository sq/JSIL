using System;

struct Foo
{
    public int X;
}

public class Program
{
    public static void Main()
    {
        var x = 100000L;

        object box = Boxed();
        Console.WriteLine(x * (long)box);

        object bbox = BoxedFoo();
        Console.WriteLine(((Foo)bbox).X);

        Console.WriteLine(((long)box).GetType());
    }

    public static object BoxedFoo()
    {
        return new Foo();
    }

    public static object Boxed()
    {
        return 634590720000000000L;
    }

}