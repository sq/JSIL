using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        var document = Builtins.Global["document"];
        var output = document.getElementById("output");
        output.value = "Hello, " + args[0] + "!\n";
    }
}