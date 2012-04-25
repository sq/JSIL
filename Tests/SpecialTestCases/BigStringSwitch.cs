using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        foreach (var arg in args) {
            switch (arg) {
                case "howdy":
                    Console.WriteLine("0");
                    break;
                case "hello":
                    Console.WriteLine("1");
                    break;
                case "world":
                    Console.WriteLine("2");
                    break;
                case "what":
                    Console.WriteLine("3");
                    break;
                case "why":
                    Console.WriteLine("4");
                    break;
                case "who":
                    Console.WriteLine("5");
                    break;
                case "where":
                    Console.WriteLine("6");
                    break;
                case "when":
                    Console.WriteLine("7");
                    break;
                default:
                    Console.WriteLine("default");
                    break;
            }
        }
    }
}