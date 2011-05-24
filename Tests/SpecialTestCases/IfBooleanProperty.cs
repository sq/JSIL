using System;

public static class Program {
    private static bool p;

    public static bool P {
        get {
            return p;
        }
        set {
            p = value;
        }
    }

    public static void Main (string[] args) {
        p = true;
        if (P)
            Console.WriteLine("true");

        p = false;
        if (!P)
            Console.WriteLine("false");
    }
}