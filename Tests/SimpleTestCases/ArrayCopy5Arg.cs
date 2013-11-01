using System;
using System.Text;

public static class Program {
    public static void Main () {
        var a = new byte[16];
        var b = new byte[] { 1, 2, 3, 4, 5, 6 };

        Array.Copy(b, 1, a, 2, 4);

        PrintBytes(a);
    }

    public static void PrintBytes (byte[] bytes) {
        var sb = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++)
            sb.AppendFormat("{0:X2}", bytes[i]);

        Console.WriteLine("{0:D3}b [{1}]", bytes.Length, sb.ToString());
    }
}