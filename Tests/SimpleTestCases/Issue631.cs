using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict["one"] = 1;
        dict["two"] = 2;
        dict["three"] = 3;

        foreach (KeyValuePair<string, int> pair in dict) {
            Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
        }
    }
}