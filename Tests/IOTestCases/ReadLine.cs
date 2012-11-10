using System;
using System.IO;

public class Program {
    public static void Main() {
        var bytes = new byte[] {
            97, 13, 10, 98, 
            10, 99, 13, 10,
            100
        };

        using (var ms = new MemoryStream(bytes, false))
        using (var sr = new StreamReader(ms, true)) {
            Util.PrintString(sr.ReadLine());
            Util.PrintString(sr.ReadLine());
            Util.PrintString(sr.ReadLine());
            Util.PrintString(sr.ReadLine());
            Util.PrintString(sr.ReadLine());
        }
    }
}