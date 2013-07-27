using System;

public static class Program {
    public static void Main () {
        try {
            Console.WriteLine(string.Format("{{0} {1}}", "hello", "world"));
        } catch (Exception exc) {
            Console.WriteLine(exc.Message);
        }
        Console.WriteLine(string.Format("{{{0} {1}}}", "hello", "world"));
    }
}
