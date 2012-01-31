using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        Func<char> ch = () => 'a';

        Console.WriteLine("cb" + ch());
        Console.WriteLine(String.Concat("cb", ch()));
    }
}