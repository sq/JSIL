using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        const string pri = "pri";
        string nt = "nt";

        var p = Builtins.Global[pri + nt] as dynamic;
        if (p != null)
            p("printed");

        if (Builtins.Local["p"] != null)
            (Builtins.Local["p"] as dynamic)("printed again");

        var q = Builtins.Global["quit"] as dynamic;
        if (q != null)
            q();

        Console.WriteLine("test");
    }
}