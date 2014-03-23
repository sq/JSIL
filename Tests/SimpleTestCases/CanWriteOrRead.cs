using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var t = typeof(Program);

        DumpProperty(t, "ReadOnly");
        DumpProperty(t, "WriteOnly");
        DumpProperty(t, "ReadWrite");
    }

    static void DumpProperty (Type type, string name) {
        var p = type.GetProperty(name);
        Console.WriteLine("{0} {1} {2}", name, p.CanRead ? 1 : 0, p.CanWrite ? 1 : 0);
    }

    public static int ReadOnly {
        get {
            return 0;
        }
    }

    public static int WriteOnly {
        set {
            ;
        }
    }

    public static int ReadWrite {
        get;
        set;
    }
}