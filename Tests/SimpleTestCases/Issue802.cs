using System;

public static class Program {
    public static void Main () {
        bool test = Convert.ToBoolean(null);
        Console.WriteLine(test.ToString().ToLower()); // Should output "false"
    }
}