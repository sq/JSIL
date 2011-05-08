using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        string a, b;

        a = "5";
        Console.WriteLine("a = {0}, b = {1}", a, b = "7");
        Console.WriteLine("a = {0}, b = {1}", a, b);
    }
}