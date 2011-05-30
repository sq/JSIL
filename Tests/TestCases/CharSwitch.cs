using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        Func<char> a = () => 'a';
        Func<string> b = () => "abcd";

        switch (a()) {
            case 'a':
                Console.WriteLine("a");
                break;
            default:
                Console.WriteLine("unknown");
                break;
        }

        switch (b()[1]) {
            case 'a':
                Console.WriteLine("a");
                break;
            case 'b':
                Console.WriteLine("b");
                break;
            case 'c':
                Console.WriteLine("c");
                break;
            case 'd':
                Console.WriteLine("d");
                break;
            default:
                Console.WriteLine("unknown");
                break;
        }
    }
}