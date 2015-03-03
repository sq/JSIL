using System;
using System.Collections.Generic;

public static class Program {
    public static void Main () {
        var list = new List<int> { 0, 1, 2, 3, 4, 5 };
        list.Reverse(1, 3);
        foreach (var member in list) {
            Console.WriteLine(member);
        }
    }
}