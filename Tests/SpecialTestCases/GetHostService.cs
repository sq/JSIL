using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        object service = null;

        try {
            service = JSIL.Services.Get("nonexistent");
            Console.WriteLine("didn't throw");
        } catch {
            Console.WriteLine("threw");
        }

        service = JSIL.Services.Get("stdout", false);
        if (service == null)
            Console.WriteLine("null");
        else
            Console.WriteLine("not null");
    }
}