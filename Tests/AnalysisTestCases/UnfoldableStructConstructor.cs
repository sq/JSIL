using System;

public static class Program {
    public static void Main (string[] args) {
        Vector2 retVec = new Vector2(
            retVec.X = 1, retVec.Y = 2
        );
        Console.WriteLine(retVec);
    }
}

public struct Vector2 {
    public float X, Y;

    public Vector2 (float x, float y) {
        X = x;
        Y = y;
    }

    public override string ToString () {
        return String.Format("({0}, {1})", X, Y);
    }
}