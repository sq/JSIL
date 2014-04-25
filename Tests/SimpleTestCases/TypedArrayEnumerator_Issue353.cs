using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var item = TestInnerArrayCast(GetItems(), it => new[] { it });
        Console.WriteLine(item.GetType().FullName);
    }

    private static IEnumerable<BaseClass> GetItems()
    {
        return new List<DerivedClass>() { new DerivedClass() };
    }

    public static BaseClass TestInnerArrayCast(IEnumerable<BaseClass> source, Func<BaseClass, IEnumerable<BaseClass>> selector)
    {
        var outerEnumerator = source.GetEnumerator();
        outerEnumerator.MoveNext();

        var inner = selector(outerEnumerator.Current);
        var innerEnumerator = inner.GetEnumerator();
        innerEnumerator.MoveNext();

        return innerEnumerator.Current;
    }
}

public class BaseClass
{
}

public class DerivedClass : BaseClass
{
}