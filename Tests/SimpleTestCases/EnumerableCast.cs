using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    private static void PrintDoubles (IEnumerable<double> doubles) {
        foreach (double d in doubles)
            Console.WriteLine(d);
    }

    public static void Main () {
        // test #1: casting of IEnumerable containg doubles to IEnumerable<double> should work
        var list = new ArrayList() { 1.23, 4.56, 7.89 };
        PrintDoubles(list.Cast<double>());

        // test #2: an exception should be thrown once the string element is reached
        list.Add("foo");
        try {
            PrintDoubles(list.Cast<double>());
            Console.WriteLine("Fail");
        } catch (InvalidCastException) {
            Console.WriteLine("Pass");
        }
    }
}