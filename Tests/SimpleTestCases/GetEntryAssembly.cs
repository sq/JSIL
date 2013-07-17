//@generateExecutable

using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var entry = Assembly.GetEntryAssembly();
        Console.WriteLine(entry);
    }
}