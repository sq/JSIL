using System;
using System.Collections.Generic;

public static class Program {
    public static void Main () {
        List<int> array = new List<int> { 1, 2, 3 };
        List<int> array2 = new List<int> { 4, 5, 6 };
        array.InsertRange(1, array2);
        foreach (var member in array)
            Console.WriteLine(member);
    }
}