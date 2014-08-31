using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var items = (IEnumerable<string>)GetTypedArray(() => new [] {"a", "b"});
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }

    public static IEnumerable<T> GetTypedArray<T>(Func<IEnumerable<T>> func)
    {
        return func();
    }
}
