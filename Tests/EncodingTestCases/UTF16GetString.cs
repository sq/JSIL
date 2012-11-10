using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        Common.PrintString(
            Encoding.Unicode.GetString(Common.UTF16Bytes)
        );

        Common.PrintString(
            (new UnicodeEncoding(false, false)).GetString(Common.UTF16Bytes)
        );

        Common.PrintString(
            (new UnicodeEncoding(true, false)).GetString(Common.UTF16BEBytes)
        );
    }
}