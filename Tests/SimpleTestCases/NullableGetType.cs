using System;

public static class Program {
    public static void Main (string[] args) {
        int? i = 1;
        float? f = null;

        try {
            Console.WriteLine(i.GetType());
        } catch (Exception exc) {
            Console.WriteLine("threw {0}", exc.GetType().Name);
        }

        try {
            Console.WriteLine(f.GetType());
        } catch (Exception exc) {
            Console.WriteLine("threw {0}", exc.GetType().Name);
        }
    }
}
