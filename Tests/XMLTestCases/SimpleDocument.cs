using System;
using System.Xml;
using System.Text;
using System.IO;
using JSIL.Meta;

public static class Program {
    [JSReplacement("JSIL.XML.ReaderFromString($xml)")]
    public static XmlReader ReaderFromString (string xml) {
        var ms = new MemoryStream(Encoding.ASCII.GetBytes(xml));
        return XmlReader.Create(ms);
    }

    public static void Main (string[] args) {
        const string xml = @"<root><child /><child><subchild /><subchild /></child></root>";

        var xr = ReaderFromString(xml);

        while (xr.Read()) {
            Console.WriteLine(xr.NodeType.ToString());
        }
    }
}