using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var list = new List<int> { 1, 3, 5, 7, 9, 11, 13 };
        var lessThanTen = list.Where((i) => i < 10);

        foreach (var i in lessThanTen)
            Console.WriteLine(i);
    }
}