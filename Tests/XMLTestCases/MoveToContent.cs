using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = @"<!-- comment1 --><root><!-- comment2 --><child>child1</child></root>";

        var xr = Common.ReaderFromString(xml);
        xr.MoveToContent();
        Console.WriteLine("{0} {1}", xr.NodeType, xr.Name);
    }
}