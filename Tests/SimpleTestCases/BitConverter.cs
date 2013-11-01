using System;
using System.Text;
using System.Collections.Generic;

public static class Program {
    public static readonly List<byte[]> Arrays = new List<byte[]>();

    public static void Main (string[] args) {
        PrintAndStore(BitConverter.GetBytes((byte)123));
        PrintAndStore(BitConverter.GetBytes((sbyte)123));
        PrintAndStore(BitConverter.GetBytes((ushort)123));
        PrintAndStore(BitConverter.GetBytes((short)123));
        PrintAndStore(BitConverter.GetBytes((uint)123));
        PrintAndStore(BitConverter.GetBytes((int)123));
        PrintAndStore(BitConverter.GetBytes((ulong)123));
        PrintAndStore(BitConverter.GetBytes((long)123));
        PrintAndStore(BitConverter.GetBytes(1.23f));
        PrintAndStore(BitConverter.GetBytes(1.23));

        Console.WriteLine(BitConverter.ToUInt16(Arrays[2], 0));
        Console.WriteLine(BitConverter.ToInt16(Arrays[3], 0));
        Console.WriteLine(BitConverter.ToUInt32(Arrays[4], 0));
        Console.WriteLine(BitConverter.ToInt32(Arrays[5], 0));
        Console.WriteLine(BitConverter.ToUInt64(Arrays[6], 0));
        Console.WriteLine(BitConverter.ToInt64(Arrays[7], 0));
        Console.WriteLine("{0:000.00}", BitConverter.ToSingle(Arrays[8], 0));
        Console.WriteLine("{0:000.00}", BitConverter.ToDouble(Arrays[9], 0));
    }

    public static void PrintAndStore (byte[] bytes) {
        var sb = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++)
            sb.AppendFormat("{0:X2}", bytes[i]);

        Console.WriteLine("{0:D3}b [{1}]", bytes.Length, sb.ToString());

        Arrays.Add(bytes);
    }
}