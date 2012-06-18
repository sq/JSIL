using System;
using System.Xml;
using System.Text;
using System.IO;
using JSIL.Meta;

public static class Common {
    public static XmlWriter MakeXmlWriter (out MemoryStream ms) {
        ms = new MemoryStream();
        return XmlWriter.Create(ms);
    }
    
    public static Encoding GetEncoding () {
        if (JSIL.Builtins.IsJavascript)
            return System.Text.Encoding.ASCII;
        else
            return new UTF8Encoding(false);
    }

    public static string UTF8ToString (MemoryStream ms) {
        var encoding = GetEncoding();
        var result = encoding.GetString(
            ms.GetBuffer(), 0, (int)ms.Length
        );

        // Fucking UTF8
        if (result[0] == 0xFEFF)
            result = result.Substring(1);

        return result;
    }
}