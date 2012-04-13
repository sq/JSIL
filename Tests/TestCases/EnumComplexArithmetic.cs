using System;

public enum FaceDirection {
    Left = -1,
    Right = 1,
}

public static class Program {
    public static void Main (string[] args) {
        var a = FaceDirection.Left;
        var b = FaceDirection.Right;

        var positionX = 32;
        var localBoundsWidth = 128;
        var tileWidth = 32;

        float posX = positionX + localBoundsWidth / 2 * (int)a;
        int tileX = (int)Math.Floor(posX / tileWidth) - (int)b;

        int ilspy = (int)((FaceDirection)Math.Floor((double)(posX / (float)tileWidth)) - b);

        Console.WriteLine("{0} {1} {2}", posX, tileX, ilspy);
    }
}