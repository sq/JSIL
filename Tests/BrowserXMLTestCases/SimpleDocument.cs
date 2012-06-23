using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = @"<root><child /><child><subchild /><subchild /></child></root>";

        var xr = Common.ReaderFromString(xml);
        Common.PrintNodes(xr);
    }
}