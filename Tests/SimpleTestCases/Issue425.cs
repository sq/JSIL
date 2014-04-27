using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Get().Equals(Get()) ? "true" : "false");
    }

    public static long Get () {
        return 1;
    }
}