using System;
using System.IO;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Path.GetExtension("abc"));
        Console.WriteLine(Path.GetExtension("abc.def"));
        Console.WriteLine(Path.GetExtension("abc.def.ghi"));
    }
}