using System;

public static class Program {
    public static string ConcatStrings (string a, string b) {
        return a + b;
    }

    public static void Main (string[] args) {
        const string xml = "<root />";

        var xr = Common.ReaderFromString(xml);
        var nameTable = xr.NameTable;

        Console.WriteLine("'{0}' '{1}'", nameTable.Add("A"), nameTable.Add("B"));

        Console.WriteLine(
            (nameTable.Add(ConcatStrings("a", "b")) == nameTable.Add(ConcatStrings("a", "b"))) ?
            "true" : "false"
        );
    }
}