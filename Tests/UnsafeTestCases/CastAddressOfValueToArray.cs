using System;

static class Program {
    unsafe static byte[] GetUIntBytes (byte* bytes) {
        return new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] };
    }

    unsafe static byte[] GetBytes (int value) {
        return GetUIntBytes((byte*)&value);
    }

    public static void Main () {
        byte[] bytes = GetBytes(0x12345678);

        foreach (var b in bytes)
            Console.WriteLine(b);
    }
}