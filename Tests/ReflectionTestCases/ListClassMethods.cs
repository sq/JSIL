using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("No public modifier");
        Common.Util.ListMethods(
            typeof(T),
            BindingFlags.Instance | BindingFlags.DeclaredOnly
        );

        Console.WriteLine("NonPublic");
        Common.Util.ListMethods(
            typeof(T),
            BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic
        );

        Console.WriteLine("Public");
        Common.Util.ListMethods(
            typeof(T),
            BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public
        );

        Console.WriteLine("NonPublic | Public");
        Common.Util.ListMethods(
            typeof(T),
            BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public
        );
    }
}

public class T {
    private void A () {
        Console.WriteLine("A()");
    }

    protected void A (object o) {
        Console.WriteLine("A(object)");
    }

    public void A (int i) {
        Console.WriteLine("A(int)");
    }

    public void B (string s) {
        Console.WriteLine("B(string)");
    }
}