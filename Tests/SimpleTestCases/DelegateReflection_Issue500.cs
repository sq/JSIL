using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var mi = typeof(object).GetMethod("GetHashCode");
        Console.WriteLine(mi.Name);

        mi = typeof(Action).GetMethod("NotExistedMethod");
        if (mi != null)
        {
            Console.WriteLine("NotExistedMethod");
        }

        mi = typeof(Action<A>).GetMethod("Invoke");
            Console.WriteLine(mi.Name);
            Console.WriteLine(mi.GetParameters()[0].ParameterType.Name);

        mi = typeof (Generic<A>.InnerDelegate).GetMethod("Invoke");
        Console.WriteLine(mi.ReturnType.Name);
    }
}

public class A {}

public static class Generic<T>
{
    public delegate T InnerDelegate();
}