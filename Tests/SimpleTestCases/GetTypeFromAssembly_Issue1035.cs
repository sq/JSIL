using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var typeInt32 = typeof(object).Assembly.GetType("System.Int32");
        var typeDictionary =
            typeof (object).Assembly.GetType(
                "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib],[System.Object, mscorlib]]");
        Console.WriteLine(
            "{0} {1}", typeInt32.Name, typeDictionary.Name
            );

    }
}