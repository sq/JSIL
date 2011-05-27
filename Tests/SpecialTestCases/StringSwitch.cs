using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        foreach (var arg in args) {
            switch (arg) {
                case "hello":
                    Console.WriteLine("world");
                    break;
                case "world":
                    Console.WriteLine("hello");
                    break;
                case "what":
                    Console.WriteLine("huh");
                    break;
                default:
                    Console.WriteLine("what");
                    break;
            }
        }
    }
}