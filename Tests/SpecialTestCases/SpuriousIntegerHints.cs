using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new Bgra4444(0, 1, 0, 1));
        Console.WriteLine(new Bgra4444(0.5f, 0.5f, 0.5f, 0.25f));
    }
}

public struct Bgra4444 {
    public readonly ushort PackedValue;

    public Bgra4444 (float x, float y, float z, float w) {
	    PackedValue = (ushort)(((int)(x * 15f) & 15) << 12 | ((int)(y * 15f) & 15) << 8 | ((int)(z * 15f) & 15) << 4 | ((int)(w * 15f) & 15));
    }

    public override string ToString () {
        return PackedValue.ToString("X4");
    }
}