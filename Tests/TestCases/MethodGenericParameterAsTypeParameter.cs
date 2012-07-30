using System;
using System.Collections.Generic;

public static class Program {
    public static List<string> MyToList1 (IEnumerable<string> source) // works OK
    {
        return new List<string>(source);
    }

    public static List<TSource> MyToList2<TSource> (IEnumerable<TSource> source) // fails
    {
        return new List<TSource>(source);
    }
    
    public static void Main (string[] args) {
        var s = new List<string>() { "zero", "one", "two" };
        Console.WriteLine("First");
        MyToList1(s);
        Console.WriteLine("Second");
        MyToList2(s);
    }
}