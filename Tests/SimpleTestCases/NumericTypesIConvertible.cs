using System;

public static class Program {
    public static void Main (string[] args) {
        WriteIConvertible('a');
        WriteIConvertible(false);
        WriteIConvertible(3d);
        WriteIConvertible(3f);
        WriteIConvertible(3m);
        WriteIConvertible(3);
        WriteIConvertible(3u);
        WriteIConvertible(3L);
        WriteIConvertible(3uL);
        WriteIConvertible(GetShort());
        WriteIConvertible(GetUShort());
        WriteIConvertible(GetByte());
        WriteIConvertible(GetSByte());

        //WriteIConvertible((short)3);
        //WriteIConvertible((ushort)3);
        //WriteIConvertible((sbyte)3);
        //WriteIConvertible((byte)3);
    }

    public static void WriteIConvertible(IConvertible input)
    {
        Console.WriteLine("{0}: {1}", input.GetTypeCode(), input.ToString(null).ToLower());
    }

    public static short GetShort()
    {
        return 3;
    }

    public static ushort GetUShort()
    {
        return 3;
    }

    public static byte GetByte()
    {
        return 3;
    }

    public static sbyte GetSByte()
    {
        return 3;
    }
}
