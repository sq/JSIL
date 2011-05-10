using System;

public static class Program {
    public static bool Method (ref object x, object y, object z) {
        Console.WriteLine("Method(<ref object>, <object>, <object>)");
        if (x == y) {
            x = z;
            return true;
        } else
            return false;
    }

    public static bool Method (ref int x, int y, int z) {
        Console.WriteLine("Method(<ref int>, <int>, <int>)");
        if (x == y) {
            x = z;
            return true;
        } else
            return false;
    }

    public static void Main (string[] args) {
        int a = 0;
        object o = null;

        Method(ref o, null, "a");
        Console.WriteLine("{0}", o);
        Method(ref o, "a", "b");
        Console.WriteLine("{0}", o);

        Method(ref a, 0, 1);
        Console.WriteLine("{0}", a);
        Method(ref a, 1, 2);
        Console.WriteLine("{0}", a);
    }
}