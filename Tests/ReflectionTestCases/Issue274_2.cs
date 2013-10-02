using System;

public static class Program {
    public static void Main () {
        var parseMethodInfo = typeof(int).GetMethod("Parse", new Type[] { typeof(string) });
        var result = parseMethodInfo.Invoke(null, new object[] { "14" });

        Console.WriteLine(result);
    }
}