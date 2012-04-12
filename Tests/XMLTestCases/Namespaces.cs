using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = "<root xmlns:ns1=\"ns1\"><child /><ns1:child /></root>";

        var xr = Common.ReaderFromString(xml);
        xr.MoveToContent();

        xr.Read();
        Console.WriteLine(
            "'{0}' '{1}' '{2}'", 
            xr.Name, xr.LocalName, xr.NamespaceURI ?? "<null>"
        );

        xr.Read();
        Console.WriteLine(
            "'{0}' '{1}' '{2}'",
            xr.Name, xr.LocalName, xr.NamespaceURI ?? "<null>"
        );
    }
}