using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Util {
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

    public static void PrintByteArray (byte[] bytes, int maxLength = int.MaxValue) {
        var sb = new StringBuilder();
        int length = Math.Min(bytes.Length, maxLength);
        for (int i = 0; i < length; i++)
            sb.AppendFormat("{0:X2}", bytes[i]);

        Console.WriteLine("{0:D3}b [{1}]", length, sb.ToString());
    }

    public static void PrintString (string str) {
        if (str == null) {
            Console.WriteLine("null");
            return;
        }

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