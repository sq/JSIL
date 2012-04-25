using System;

public static class Program {
    public static void Main (string[] args) {
        var arr = "ABCD".ToCharArray();

        foreach (var ch in arr)
            Console.WriteLine(ch);
    }
}