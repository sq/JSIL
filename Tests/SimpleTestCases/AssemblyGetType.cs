using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var customType = executingAssembly.GetType("CustomType");
        Console.WriteLine(customType);
    }
}

public class CustomType {
}