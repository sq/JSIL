using System;

public enum FaceDirection {
    Left = -1,
    Right = 1,
}

public static class Program {
    public static void Main (string[] args) {
        var a = FaceDirection.Left;
        var b = FaceDirection.Right;

        Console.WriteLine("{0} {1}", a, b);
        a = (FaceDirection)(-((int)a));
        b = (FaceDirection)(-((int)b));
        Console.WriteLine("{0} {1}", a, b);
    }
}