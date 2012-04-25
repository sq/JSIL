using System;

public static class Program {
    public static void Main (string[] args) {
        Func<char> a = () => 'a', 
            b = () => 'b', 
            c = () => 'c';

        Console.WriteLine(c() - a());
        Console.WriteLine(a() + 2);
        Console.WriteLine(a() + c() - a());
    }
}