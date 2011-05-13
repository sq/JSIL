using System;

public static class Program {
    public static int @if;

    public static void Increment (ref int x) {
        x += 1;
    }

    public static void Main (string[] args) {
        Console.WriteLine("{0}", @if);
        Increment(ref @if);
        Console.WriteLine("{0}", @if);
    }
}