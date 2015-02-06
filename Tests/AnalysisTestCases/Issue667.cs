using System;
using System.Collections.Generic;

public static class Program {
    public static void SwapParameters (int a, int b) {
        if (a > b) {
            int swap = a;
            //Console.WriteLine("swap = {0}", swap);
            a = b;
            b = swap;
        }
        Console.WriteLine("a = {0}", a);
        Console.WriteLine("b = {0}", b);
    }

    public static void SwapLocals (int a, int b) {
        int _a = a;
        int _b = b;

        if (_a > _b) {
            int swap = _a;
            //Console.WriteLine("swap = {0}", swap);
            _a = _b;
            _b = swap;
        }

        Console.WriteLine("a = {0}", _a);
        Console.WriteLine("b = {0}", _b);
    }

    public static void Main (string[] args) {
        SwapParameters(6, 5);
        SwapLocals(7, 3);
    }
}