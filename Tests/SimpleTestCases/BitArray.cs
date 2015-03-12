using System;
using System.Collections;

public static class Program { 
    public static void Main (string[] args) {
        var b = new BitArray(5);
        b[0] = true;
        b[1] = false;

        for (var i = 0; i < b.Length; i += 1) {
            Console.WriteLine("{0}: {1}", i, b[i]);
        }
    }
}