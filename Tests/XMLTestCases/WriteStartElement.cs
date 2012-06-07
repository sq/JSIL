using System;
using System.IO;
using System.Xml;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        var ms = new MemoryStream();
        var xw = XmlWriter.Create(ms);

        xw.WriteStartElement("elt");
        xw.WriteEndElement();

        xw.Close();
        Console.WriteLine(Encoding.UTF8.GetString(ms.GetBuffer()));
    }
}