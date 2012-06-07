using System;
using System.IO;
using System.Xml;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        var utf = new UTF8Encoding(false);
        var ms = new MemoryStream();
        var xw = XmlWriter.Create(ms);

        xw.WriteStartElement("elt");
        xw.WriteEndElement();

        xw.Close();

        var result = utf.GetString(
            ms.GetBuffer(), 0, (int)ms.Length
        );

        // Fucking UTF8
        if (result[0] == 0xFEFF)
            result = result.Substring(1);

        Console.WriteLine(result);
    }
}