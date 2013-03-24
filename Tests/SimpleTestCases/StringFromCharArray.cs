using System;

public static class Program {
    public static void Main (string[] args) {
        var chars = new char[] { 'a', 'b', 'c', '\0' };
        var str = new string(chars);

        Console.WriteLine(str.Length);
        Console.WriteLine(str[0]);
        Console.WriteLine(str.Substring(0, 3));
        Console.WriteLine((int)str[3]);
    }
}