using System;

public static class Program {
    public static void Main (string[] args) {
        // NOTE: Empty elements are treated as the same as elements with no children by the DOM.
        // Thus, <child></child> can't be in this test.
        const string xml = @"<root><child>hello</child><child>world</child><child /></root>";

        var xr = Common.ReaderFromString(xml);
        xr.ReadStartElement();

        Console.WriteLine(xr.ReadElementContentAsString() ?? "<null>");

        Console.WriteLine(xr.ReadElementContentAsString() ?? "<null>");

        Console.WriteLine(xr.ReadElementContentAsString() ?? "<null>");
    }
}