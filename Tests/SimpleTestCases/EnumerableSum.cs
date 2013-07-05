using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var list = new List<float> { 1, 3.5f, 5, 7.77f, 9, 11.11111f, 13 };
        var list2 = new List<int> { 1, 5, 9, 22 };

        Console.WriteLine("{0:0000.0000}", list.Sum());
        Console.WriteLine("{0:0000}", list2.Sum());
    }
}