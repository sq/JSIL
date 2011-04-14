using System;

public static class Program {
    public static void Main (string[] args) {
        try {
            throw new Exception("Error");
        } catch (Exception ex) {
            // We have to limit ourselves to the first line of the exception string,
            //  because JS exceptions don't have consistent support for tracebacks
            var text = ex.ToString().Split('\n')[0];
            Console.WriteLine("Caught: {0}", text);
        }
    }
}