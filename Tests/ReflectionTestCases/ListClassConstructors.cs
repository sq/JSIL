using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListConstructors(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );
        Common.Util.ListConstructors(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public 
        );
        Common.Util.ListConstructors(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic
        );
    }
}

public class T {
    public T () {
        Console.WriteLine("T()");
    }

    public T (int i) {
        Console.WriteLine("T(int)");
    }

    public T (string s) {
        Console.WriteLine("T(string)");
    }
}