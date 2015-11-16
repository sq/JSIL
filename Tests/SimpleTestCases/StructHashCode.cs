using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args)
    {
        var s1 = new Pair<int, int>(1, 1);
        var s2 = new Pair<int, int>(1, 1);
        var s3 = new Pair<int, int>(1, 2);

        var obj1 = new object();
        var obj2 = new object();

        var s4 = new Pair<object, object>(null, obj1);
        var s5 = new Pair<object, object>(null, obj1);
        var s6 = new Pair<object, object>(null, obj2);
        var s7 = new Pair<object, object>(null, obj2);
        var s8 = new Pair<object, object>(null, null);
        var s9 = new Pair<object, object>(null, null);

        Console.WriteLine(s1.GetHashCode() == s2.GetHashCode() ? "true" : "false");
        Console.WriteLine(s1.GetHashCode() == s3.GetHashCode() ? "true" : "false");

        Console.WriteLine(s4.GetHashCode() == s5.GetHashCode() ? "true" : "false");
        Console.WriteLine(s4.GetHashCode() == s6.GetHashCode() ? "true" : "false");
        Console.WriteLine(s4.GetHashCode() == s8.GetHashCode() ? "true" : "false");

        Console.WriteLine(s6.GetHashCode() == s7.GetHashCode() ? "true" : "false");
        Console.WriteLine(s8.GetHashCode() == s9.GetHashCode() ? "true" : "false");
    }
}

public struct Pair<T, K>
{
    private T _f1;
    private K _f2;

    public Pair(T f1, K f2)
    {
        _f1 = f1;
        _f2 = f2;
    }
}