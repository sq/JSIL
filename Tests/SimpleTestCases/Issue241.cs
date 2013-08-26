using System;
using System.Collections.Generic;
using System.Linq;

public static class Program { 
    public static void Main (string[] args) {
        int i1 = 1;
        int i2 = 2;
        bool r = i1.Equals(i2);
        Console.WriteLine(r ? "true" : "false");
    }
}