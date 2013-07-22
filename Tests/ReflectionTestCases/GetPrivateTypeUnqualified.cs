using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var t1 = Type.GetType("N.P1");
        var t2 = Type.GetType("N.P2");
        var t3 = Type.GetType("System.String");
        var t4 = Type.GetType("System.Int32");

        Console.WriteLine("{0} {1} {2} {3}", t1, t2, t3, t4);
    }
}

namespace N {
    class P1 {
    }

    struct P2 {
    }
}