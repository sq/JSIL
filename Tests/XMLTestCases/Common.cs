using System;
using System.Xml;
using System.Text;
using System.IO;
using JSIL.Meta;

public static class Common {
    [JSReplacement("JSIL.XML.ReaderFromString($xml)")]
    public static XmlReader ReaderFromString (string xml) {
        var ms = new MemoryStream(Encoding.ASCII.GetBytes(xml));
        return XmlReader.Create(ms);
    }

    public static void PrintNodesCore (XmlReader reader, Action onNode) {
        int count = 0;

        while (reader.Read()) {
            count += 1;
            onNode();
        }

        Console.WriteLine("// {0} node(s)", count);
    }

    public static void PrintNodes (XmlReader reader) {
        PrintNodesCore(reader,
            () => Console.WriteLine(
                "{0}{1} {2}",
                reader.NodeType.ToString(),
                reader.IsEmptyElement ? " Empty" : "",
                reader.Name ?? "<null>"
            )
        );
    }

    public static void PrintNodeText (XmlReader reader) {
        PrintNodesCore(reader,
            () => Console.WriteLine(
                "{0}{1} {2}{3}",
                reader.NodeType.ToString(),
                reader.IsEmptyElement ? " Empty" : "",
                reader.Name == null ? "" : reader.Name,
                String.IsNullOrEmpty(reader.Value) ? "" : "'" + reader.Value + "'"
            )
        );
    }

    public static void PrintNodeAttributeCounts (XmlReader reader) {
        PrintNodesCore(reader,
            () => {
                Console.WriteLine(
                    "{0}{1} {2}{3}",
                    reader.NodeType.ToString(),
                    reader.IsEmptyElement ? " Empty" : "",
                    reader.Name == null ? "" : reader.Name + " ",
                    reader.AttributeCount
                );
            }
        );
    }
}