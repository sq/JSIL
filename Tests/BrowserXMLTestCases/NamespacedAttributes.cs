using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = "<root xmlns:ns1=\"ns1\" attr1=\"a\" ns1:attr2=\"b\" />";

        var xr = Common.ReaderFromString(xml);
        xr.MoveToContent();

        Console.WriteLine(
            "'{0}' '{1}'",
            xr.GetAttribute("attr1", "ns1") ?? "<null>",
            xr.GetAttribute("attr2", "ns1") ?? "<null>"
        );
    }
}