using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        const string pri = "pri";
        string nt = "nt";

        var p = Builtins.Global[pri + nt];
        if (p != null)
            p("printed");

        if (Builtins.Local["p"] != null)
            Builtins.Local["p"]("printed again");

        var q = Builtins.Global["quit"];
        if (q != null)
            q();

        Console.WriteLine("test");
    }
}