using System;

public static class Program {
    public static void Main() {
        int x = 5;
        if (typeof(object).IsAssignableFrom(x.GetType())) {
            Console.WriteLine("Passed");
        };
    }
}