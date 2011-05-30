using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        string a = "a";
        string b = "b";
        string c = "c";
        object d = "d";
        object e = "e";

        Console.WriteLine(a + b + c);
        Console.WriteLine(String.Concat(d, e));
        Console.WriteLine(String.Concat(new object[] {
            "a", "b", 5, "d"
        }));
    }
}