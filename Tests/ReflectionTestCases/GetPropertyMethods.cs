using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var t = typeof(T);
        var prop = t.GetProperty("Property");

        Console.WriteLine(prop.GetGetMethod().Name);
        Console.WriteLine(prop.GetSetMethod().Name);
    }
}

public class T {
    public int Property { get; set; }
}