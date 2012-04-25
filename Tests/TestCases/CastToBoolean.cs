using System;
using System.Collections;
using System.Collections.Generic;

public static class Program
{
    public static T f<T> (T x) {
        return x;
    }

    public static void PrintBoolean (bool b) {
        Console.WriteLine(b ? "True" : "False");
    }

    public static void Main(string[] args)
    {
        PrintBoolean(f(0) != 0 && f(1) != 0);
        PrintBoolean(f(0) != 0 || f(1) != 0);
    }
}