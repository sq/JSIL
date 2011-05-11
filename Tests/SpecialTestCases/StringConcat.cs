using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        string a = "a";
        string b = "b";
        string c = "c";
        object d = "d";
        object e = "e";

        (JSIL.Builtins.Global["print"] as dynamic)(a + b + c);
        (JSIL.Builtins.Global["print"] as dynamic)(String.Concat(d, e));
    }
}