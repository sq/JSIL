using System;
using System.IO;
using System.Xml;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        MemoryStream ms;
        XmlWriter xw = Common.MakeXmlWriter(out ms);

        xw.WriteStartElement("elt");
        xw.WriteEndElement();

        xw.Close();

        Console.WriteLine(Common.UTF8ToString(ms));
    }
}