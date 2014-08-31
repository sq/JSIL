using System;

public static class Program {
    public static void Main (string[] args) {
    }

    private static byte ReadByte () {
        return 1;
    }

    private static ushort ReadUInt16 () {
        return 2;
    }

    private static uint ReadUInt32 () {
        return 3;
    }

    private static void ConvertRgb565ToRgb888 (ushort color, out byte r, out byte g, out byte b) {
        int temp;

        temp = (color >> 11) * 255 + 16;
        r = (byte)((temp / 32 + temp) / 32);
        temp = ((color & 0x07E0) >> 5) * 255 + 32;
        g = (byte)((temp / 64 + temp) / 64);
        temp = (color & 0x001F) * 255 + 16;
        b = (byte)((temp / 32 + temp) / 32);
    }

    private static void DecompressDxt3Block (int x, int y, int blockCountX, int width, int height, byte[] imageData) {
        byte a0 = ReadByte();
        byte a1 = ReadByte();
        byte a2 = ReadByte();
        byte a3 = ReadByte();
        byte a4 = ReadByte();
        byte a5 = ReadByte();
        byte a6 = ReadByte();
        byte a7 = ReadByte();

        ushort c0 = ReadUInt16();
        ushort c1 = ReadUInt16();

        byte r0, g0, b0;
        byte r1, g1, b1;
        ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
        ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

        uint lookupTable = ReadUInt32();

        int alphaIndex = 0;
        for (int blockY = 0; blockY < 4; blockY++) {
            for (int blockX = 0; blockX < 4; blockX++) {
                byte r = 0, g = 0, b = 0, a = 0;

                uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                switch (alphaIndex) {
                    case 0:
                        a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4));
                        break;
                    case 1:
                        a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
                        break;
                    case 2:
                        a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4));
                        break;
                    case 3:
                        a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
                        break;
                    case 4:
                        a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4));
                        break;
                    case 5:
                        a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
                        break;
                    case 6:
                        a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4));
                        break;
                    case 7:
                        a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
                        break;
                    case 8:
                        a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4));
                        break;
                    case 9:
                        a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
                        break;
                    case 10:
                        a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4));
                        break;
                    case 11:
                        a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
                        break;
                    case 12:
                        a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4));
                        break;
                    case 13:
                        a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
                        break;
                    case 14:
                        a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4));
                        break;
                    case 15:
                        a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
                        break;
                }
                ++alphaIndex;

                switch (index) {
                    case 0:
                        r = r0;
                        g = g0;
                        b = b0;
                        break;
                    case 1:
                        r = r1;
                        g = g1;
                        b = b1;
                        break;
                    case 2:
                        r = (byte)((2 * r0 + r1) / 3);
                        g = (byte)((2 * g0 + g1) / 3);
                        b = (byte)((2 * b0 + b1) / 3);
                        break;
                    case 3:
                        r = (byte)((r0 + 2 * r1) / 3);
                        g = (byte)((g0 + 2 * g1) / 3);
                        b = (byte)((b0 + 2 * b1) / 3);
                        break;
                }
            }
        }
    }
}