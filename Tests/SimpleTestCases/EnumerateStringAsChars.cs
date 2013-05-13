using System;
using System.Collections.Generic;

public static class Program {
    public static void Main () {
        var str = "abcdefgh";
        var strChars = (str as IEnumerable<char>);

        foreach (var ch in strChars)
            Console.WriteLine(ch);
    }
}