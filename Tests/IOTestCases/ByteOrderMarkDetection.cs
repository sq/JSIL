using System;
using System.IO;

public class Program {
    public static void Main() {
        var UTF8Bytes = new byte[] {
            239, 187, 191, 104, 105
        };
        var UTF16LEBytes = new byte[] {
            255, 254, 104, 0, 105, 0
        };
        var UTF16BEBytes = new byte[] {
            254, 255, 0, 104, 0, 105
        };

        Console.WriteLine("UTF8");
        DumpDecodedVersion(UTF8Bytes);
        Console.WriteLine("UTF16LE");
        DumpDecodedVersion(UTF16LEBytes);
        Console.WriteLine("UTF16BE");
        DumpDecodedVersion(UTF16BEBytes);
    }

    private static void DumpDecodedVersion (byte[] bytes) {
        using (var ms = new MemoryStream(bytes, false))
        using (var sr = new StreamReader(ms, true))
            Util.PrintString(sr.ReadToEnd());
    }
}