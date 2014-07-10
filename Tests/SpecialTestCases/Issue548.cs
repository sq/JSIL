using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        var obj1 = Verbatim.Expression("{}");
        var obj2 = Verbatim.Expression("{obj1: $0}", obj1);
    }
}