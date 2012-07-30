using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("key1", "value1");
        dict.Add("key2", "value2");

        foreach (var kvp in dict) {
            Console.WriteLine(kvp);
        }
    }
}