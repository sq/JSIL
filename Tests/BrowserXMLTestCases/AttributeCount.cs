using System;

public static class Program {
    public static void Main (string[] args) {
        // NOTE: The first child node needs text content so the DOM doesn't flag it as empty.
        const string xml = "<root><child attr1=\"a\" attr2=\"b\">child1</child><child attr2=\"c\" attr1=\"d\" /><child /></root>";

        var xr = Common.ReaderFromString(xml);
        Common.PrintNodeAttributeCounts(xr);
    }
}