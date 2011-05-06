using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        int i = 0;

        i += 1;
        i = i + 1;
        Console.WriteLine("{0}", i);
        Console.WriteLine("{0}", i = i + 1);
        i -= 1;
        i = i - 1;
        Console.WriteLine("{0}", i);
        Console.WriteLine("{0}", i = i - 1);
        Console.WriteLine("{0}", i);
    }
}