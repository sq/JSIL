using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var asm = Assembly.GetExecutingAssembly();
        var t1 = asm.GetType("N.P1");
        var t2 = asm.GetType("N.P2");
        var t3 = asm.GetType("System.String");
        var t4 = asm.GetType("System.Int32");

        Console.WriteLine("{0} {1} {2} {3}", t1, t2, t3, t4);
    }
}

namespace N {
    class P1 {
    }

    struct P2 {
    }
}