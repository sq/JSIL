using System;

public static class Program {
    public static void Main (string[] args) {
        A();
        A(1 as dynamic);
        A("a" as dynamic);
    }

    public static void A () {
        Console.WriteLine("a(<void>)");
    }

    public static void A (int i) {
        Console.WriteLine("a(<int>)");
    }

    public static void A (string s) {
        Console.WriteLine("a(<string>)");
    }
}