using System;
using System.Text.RegularExpressions;

public static class Program {
    public static void Main () {
        var regex = new Regex("[A-Za-z]*");
        var text = "Hello, World!";
        var match = regex.Match(text);
        Console.WriteLine("{0} {1}", match.Success, match.Groups[0].Value);
    }
}