using System;
using System.Collections.Generic;

public class Program {
    public static void Main (string[] args) {
        HashSet<object> set = new HashSet<object>();
        set.Add("One");
        set.Add("Two");
        set.Add("Three");

        foreach (var item in set) {
            Console.WriteLine(item);
        }

    }
}