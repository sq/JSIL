using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        int a = 65;
        byte b = 67;
        var c = (char)a;
        var d = (char)b;

        Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);

        Func<char> e = () => 'a';
        Func<string> f = () => "a";
        Func<char> g = () => '\0';
        Func<string> h = () => "\0";

        Console.WriteLine("{0} {1} {2} {3}", e(), f(), f()[0], f() + e());

        Console.WriteLine("{0} {1}", e() == e(), g() == h()[0]);
    }
}