using System;
using System.IO;

public class Program {
    public static void Main() {
        byte[] bytes;

        var values = new long[] {
            0, 1, 0xFFFF, 0xFF00FF00FF00L, 12345678901234L, -0xFFFF, -0xFF00FF00FF00L
        };

        long length;

        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms)) {
            foreach (var value in values) {
                bw.Write(value);
                Console.WriteLine(value);
            }

            bw.Flush();
            length = ms.Position;

            bytes = ms.GetBuffer();
        }

        Util.PrintByteArray(bytes, (int)length);

        using (var ms = new MemoryStream(bytes, false))
        using (var br = new BinaryReader(ms)) {
            for (int i = 0; i < values.Length; i++) {
                var value = br.ReadInt64();
                Console.WriteLine(value);
            }
        }
    }
}