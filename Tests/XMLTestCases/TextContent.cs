using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = @"<root><child>hello</child><child>world</child><child></child><child /></root>";

        var xr = Common.ReaderFromString(xml);
        Common.PrintNodeText(xr);
    }
}