using System;

public static class Program {
    public static void Main (string[] args) {
        const string xml = @"<root><child><subchild /><subchild /></child><child><subchild /></child></root>";

        var xr = Common.ReaderFromString(xml);

        xr.MoveToContent();
        xr.Read();
        Console.WriteLine("{0} {1}", xr.NodeType.ToString(), xr.Name);

        xr.Skip();
        Console.WriteLine("{0} {1}", xr.NodeType.ToString(), xr.Name);

        xr.Skip();
        Console.WriteLine("{0} {1}", xr.NodeType.ToString(), xr.Name);
    }
}