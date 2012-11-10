using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        var encodings = new Encoding[] {
            Encoding.Unicode, new UnicodeEncoding(false, false), new UnicodeEncoding(true, false)
        };

        foreach (var e in encodings) {
            Common.PrintByteArray(
                e.GetBytes(Common.ASCIIString)
            );

            Common.PrintByteArray(
                e.GetBytes(Common.UTF8String)
            );
        }
    }
}