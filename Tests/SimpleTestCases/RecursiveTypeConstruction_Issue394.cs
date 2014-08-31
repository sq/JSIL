using System;

public static class Program
{
    public class FooBase
    {
        protected static Type Type = typeof(Foo);
    }

    public class Foo : FooBase
    {
        public Foo()
        {
            Console.WriteLine("Type=" + Type);
        }
    }

    public static void Main()
    {
        new Foo();
    }
}