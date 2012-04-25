using System;

public static class Program {
    public static void Main (string[] args) {
        var s = "abcdefabcdef";

        Console.WriteLine(s.LastIndexOf('a'));
        Console.WriteLine(s.LastIndexOf('b'));

        // FIXME
        /*
        Console.WriteLine(s.LastIndexOf('a', 5));
        Console.WriteLine(s.LastIndexOf('b', 5));
        Console.WriteLine(s.LastIndexOf('a', 1));
        Console.WriteLine(s.LastIndexOf('b', 1));
         */

        Console.WriteLine(s.LastIndexOf('w'));

        Console.WriteLine(s.LastIndexOf("abc"));
        Console.WriteLine(s.LastIndexOf("bcd"));

        // FIXME
        /*
        Console.WriteLine(s.LastIndexOf("abc", 5));
        Console.WriteLine(s.LastIndexOf("bcd", 5));
        Console.WriteLine(s.LastIndexOf("abc", 1));
        Console.WriteLine(s.LastIndexOf("bcd", 1));
         */

        Console.WriteLine(s.LastIndexOf("what"));
    }
}