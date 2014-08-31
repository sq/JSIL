using System;

public static class Program {
    public static void Main (string[] args) {
    }

    public static void Convert (int x, int y, int pitch, int bytesPerPixel) {
        byte[] levelData = new byte[2048];
		int color = BitConverter.ToInt32(levelData, y * pitch + x * bytesPerPixel);
		levelData[y * pitch + x * 4] = (byte)(((color >> 16) & 0xff)); //R:=W
		levelData[y * pitch + x * 4 + 1] = (byte)(((color >> 8) & 0xff)); //G:=V
		levelData[y * pitch + x * 4 + 2] = (byte)(((color) & 0xff)); //B:=U
		levelData[y * pitch + x * 4 + 3] = (byte)(((color >> 24) & 0xff)); //A:=Q
    }
}