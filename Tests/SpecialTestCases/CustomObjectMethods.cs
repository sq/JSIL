using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
    }
}

public enum Directions {
}

public struct TileIndex {
    public readonly int X;
    public readonly int Y;
    public readonly int Size;
    public int FrameCount;
    public int FrameDelay;
    public readonly Directions Rotation;

    public override bool Equals (object obj) {
        if (obj == null)
            return false;
        TileIndex ti = (TileIndex)obj;
        if (ti.X == X && ti.Y == Y)
            return true;

        return base.Equals(obj);
    }

    public static bool operator == (TileIndex a, TileIndex b) {
        return a.Equals(b);
    }
    public static bool operator != (TileIndex a, TileIndex b) {
        return !a.Equals(b);
    }

    public override int GetHashCode () {
        return (X ^ Y);
    }

    public override string ToString () {
        return "[" + X + "," + Y + "]";
    }
}
