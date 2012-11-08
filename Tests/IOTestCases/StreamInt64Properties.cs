using System;
using System.IO;

public class Program {
    private static void PrintStatus (Stream stream) {
        object position = stream.Position;
        object length = stream.Length;
        Console.WriteLine("position={0} {1} length={2} {3}", position.GetType(), position, length.GetType(), length);
    }

    public static void Main() {
        var ms = new MemoryStream();
        ms.Write(new byte[] { 0, 1, 2, 3 }, 0, 4);

        ms.Seek((Int64)0, SeekOrigin.Begin);
        PrintStatus(ms);

        ms.Seek((Int64)0, SeekOrigin.End);
        PrintStatus(ms);

        ms.Seek((Int64)(-2), SeekOrigin.Current);
        PrintStatus(ms);
    }
}