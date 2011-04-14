using System;

public static class Program {
    public static void Main (string[] args) {
        try {
            throw new Exception("Error");
        } catch (Exception ex) {
            var text = ex.ToString().Split('\n')[0];
            Console.WriteLine("Caught: {0}", text);
        }
    }
}