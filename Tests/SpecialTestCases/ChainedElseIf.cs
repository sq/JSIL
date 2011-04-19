using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        var i = int.Parse(args[0]);

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