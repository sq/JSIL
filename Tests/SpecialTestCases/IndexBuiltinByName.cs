using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        var print = Builtins.Global["pr" + "int"];
        if (print != null)
            print("printed");

        if (Builtins.Local["print"] != null)
            Builtins.Local["print"]("printed again");

        var quit = Builtins.Global["quit"];
        if (quit != null)
            quit();

        Console.WriteLine("test");
    }
}