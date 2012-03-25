using System;

public static class Program {
    public static void Main (string[] args) {
        var s = "abcdefabcdef";

        Console.WriteLine(s.IndexOf('a'));
        Console.WriteLine(s.IndexOf('b'));
        Console.WriteLine(s.IndexOf('a', 5));
        Console.WriteLine(s.IndexOf('b', 5));
        Console.WriteLine(s.IndexOf('a', 1));
        Console.WriteLine(s.IndexOf('b', 1));
        Console.WriteLine(s.IndexOf('w'));

        Console.WriteLine(s.IndexOf("abc"));
        Console.WriteLine(s.IndexOf("bcd"));
        Console.WriteLine(s.IndexOf("abc", 5));
        Console.WriteLine(s.IndexOf("bcd", 5));
        Console.WriteLine(s.IndexOf("abc", 1));
        Console.WriteLine(s.IndexOf("bcd", 1));
        Console.WriteLine(s.IndexOf("what"));
    }
}