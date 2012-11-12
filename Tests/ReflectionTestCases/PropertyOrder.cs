using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
            Console.WriteLine(property.Name);
    }
}

public class T {
    public int PropertyC { get; set; }
    public int PropertyB { get; set; }
    public int PropertyA { get; set; }
}