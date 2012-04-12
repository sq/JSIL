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

    public static void PrintNodes (XmlReader reader) {
        int count = 0;

        while (reader.Read()) {
            count += 1;
            Console.WriteLine(
                "{0}{1} {2}", 
                reader.NodeType.ToString(), 
                reader.IsEmptyElement ? " Empty" : "", 
                reader.Name
            );
        }

        Console.WriteLine("// {0} node(s)", count);
    }
}