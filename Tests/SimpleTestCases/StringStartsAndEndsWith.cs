using System;

public static class Program {
    public static void PrintBool (bool b) {
        Console.WriteLine(b ? 1 : 0);
    }

    public static void Main (string[] args) {
        PrintBool("abcdef".StartsWith("abc"));
        PrintBool("abcdef".StartsWith("bcd"));
        PrintBool("abcdef".EndsWith("def"));
        PrintBool("abcdef".EndsWith("cde"));
    }
}