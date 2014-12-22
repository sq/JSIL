using System;

public static class Program {
    public static void Main() {
        try {
            int.Parse("invalid int");
        }
        catch (FormatException e) {
            Console.Write("Passed");
        }
    }
}