using System;
using System.Text;
using JSIL.Meta;

public static class Common {
    public static string ASCIIString {
        get {
            return "ASCII\r\n\t\0".Normalize();
        }
    }

    public static string UTF8String {
        get {
            return "κόσμε\r\n\t\0".Normalize();
        }
    }

    public static readonly byte[] ASCIIBytes = new byte[] {
        0x41, 0x53, 0x43, 0x49, 0x49, 0x0D, 0x0A, 0x09, 0x00
    };

    public static readonly byte[] UTF8Bytes = new byte[] {
        206, 186, 207, 140,
        207, 131, 206, 188,
        206, 181, 13, 10,
        9, 0
    };

    public static string EscapeCharacter (char ch) {
        switch (ch) {
            case '\0':
                return @"\0";
            case '\a':
                return @"\a";
            case '\b':
                return @"\b";
            case '\f':
                return @"\f";
            case '\n':
                return @"\n";
            case '\r':
                return @"\r";
            case '\t':
                return @"\t";
            case '\v':
                return @"\v";
            case '\'':
                return @"\'";
            case '\"':
                return "\\\"";
            case '\\':
                return @"\\";
            default:
                if (ch <= 255)
                    return String.Format("\\x{0:X2}", (int)ch);
                else
                    return String.Format("\\u{0:X4}", (int)ch);
        }
    }

    public static void PrintByteArray (byte[] bytes) {
        var sb = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++)
            sb.AppendFormat("{0:X2}", bytes[i]);

        Console.WriteLine("{0:D3}b [{1}]", bytes.Length, sb.ToString());
    }

    public static void PrintString (string str) {
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++) {
            var ch = str[i];

            if ((ch >= 32) && (ch <= 127)) {
                sb.Append(ch);
            } else {
                sb.Append(EscapeCharacter(ch));
            }
        }

        Console.WriteLine("{0:D3}ch [{1}]", str.Length, sb.ToString());
    }
}