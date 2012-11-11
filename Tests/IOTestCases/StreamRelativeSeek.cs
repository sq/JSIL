using System;
using System.IO;

public class Program {
    private static void PrintStatus (Stream stream) {
        Console.WriteLine("position={0} length={1}", (int)stream.Position, (int)stream.Length);
    }

    public static void Main() {
        var ms = new MemoryStream();
        ms.Write(new byte[] { 0, 1, 2, 3 }, 0, 4);
        PrintStatus(ms);

        ms.Seek(-2, SeekOrigin.Current);
        PrintStatus(ms);
    }
}