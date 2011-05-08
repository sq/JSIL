using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(
            "{0}, {1}, {2}",
            1.ToString().ToString(),
            (2.5).ToString().ToString(),
            (3m).ToString().ToString()
        );
    }
}