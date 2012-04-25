using System;

public static class Program {
    public static void A () {
        Console.WriteLine("A()");
    }

    public static void A (int i) {
        Console.WriteLine("A({0})", i);
    }

    public static void A (int i, string s) {
        Console.WriteLine("A({0}, {1})", i, s);
    }

    public static void B () {
        Console.WriteLine("B()");
    }

    public static void B (int i) {
        Console.WriteLine("B(int {0})", i);
    }

    public static void B (string s) {
        Console.WriteLine("B(string {0})", s);
    }

    public static void Main (string[] args) {
        A();
        A(1);
        A(1, "str");
        B();
        B(1);
        B("str");
    }
}