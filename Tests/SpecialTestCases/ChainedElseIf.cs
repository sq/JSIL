using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        Func<string> two = () => "2";

        var i = int.Parse(two());

        if (i == 0) {
            Console.WriteLine("Zero");
        } else if (i == 1) {
            Console.WriteLine("One");
        } else if (i == 2) {
            Console.WriteLine("Two");
        } else if (i == 3) {
            Console.WriteLine("Three");
        } else {
            Console.WriteLine("Unknown");
        }
    }
}