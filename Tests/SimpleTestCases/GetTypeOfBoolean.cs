using System;

public static class Program {
    public static void Main (string[] args) {
        bool bFalse = false;
        object oFalse = false;

        Console.WriteLine(
            "{0} {1}", bFalse.GetType(), oFalse.GetType()
        );
    }
}