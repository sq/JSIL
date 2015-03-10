using System;

public static class Program {
    public static void PrintCompared (string a, string b, StringComparison comparison) {
        var result = String.Compare(a, b, comparison);
        if (result > 0)
            result = 1;
        else if (result < 0)
            result = -1;
        Console.WriteLine(result);
    }

    public static void Main (string[] args) {
        PrintCompared("asd", "asd", StringComparison.Ordinal);
        PrintCompared("asd", "asd", StringComparison.OrdinalIgnoreCase);
        PrintCompared("Asd", "asd", StringComparison.Ordinal);
        PrintCompared("Asd", "asd", StringComparison.OrdinalIgnoreCase);
        PrintCompared("Asd", "asdf", StringComparison.Ordinal);
        PrintCompared("Asd", "asdf", StringComparison.OrdinalIgnoreCase);
        PrintCompared("asd", "asdf", StringComparison.Ordinal);
        PrintCompared("asd", "asdf", StringComparison.OrdinalIgnoreCase);

        PrintCompared(null, "asd", StringComparison.Ordinal);
        PrintCompared("asd", null, StringComparison.Ordinal);
        PrintCompared(null, "asd", StringComparison.OrdinalIgnoreCase);
        PrintCompared("asd", null, StringComparison.OrdinalIgnoreCase);

        PrintCompared(null, string.Empty, StringComparison.Ordinal);
        PrintCompared(string.Empty, null, StringComparison.Ordinal);
        PrintCompared(null, string.Empty, StringComparison.OrdinalIgnoreCase);
        PrintCompared(string.Empty, null, StringComparison.OrdinalIgnoreCase);

        PrintCompared(null, null, StringComparison.Ordinal);
        PrintCompared(null, null, StringComparison.OrdinalIgnoreCase);
    }
}