using System;

public static class Program {
    public const int ShockwaveVelocity = 384;
    public const int KinematicObstructionGridSize = 8;

    public static void Main (string[] args) {
        var body = new Body();

        Blast(body);
    }

    public static void Blast (Body exploder) {
        Shockwave shockwave = new Shockwave();

        int x = (exploder.PositionPixel.X / KinematicObstructionGridSize) * KinematicObstructionGridSize;
        int y = (exploder.PositionPixel.Y / KinematicObstructionGridSize) * KinematicObstructionGridSize;

        shockwave.WarpTo(x, y);

        shockwave.TryMove(Directions.Right, ShockwaveVelocity);
        shockwave.WarpTo(x, y);
        shockwave.TryMove(Directions.Up, ShockwaveVelocity);
        shockwave.WarpTo(x, y);
        shockwave.TryMove(Directions.Left, ShockwaveVelocity);
        shockwave.WarpTo(x, y);
        shockwave.TryMove(Directions.Down, ShockwaveVelocity);
    }
}

public enum Directions {
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}

public struct Point {
    public int X, Y;

    public Point (int x, int y) {
        X = x;
        Y = y;
    }
}

public class Body {
    public Body () {
    }

    public Point PositionPixel {
        get {
            return new Point(76, 82);
        }
    }
}

public class Shockwave {
    public Shockwave () {
    }

    public void WarpTo (int x, int y) {
        Console.WriteLine("Shockwave.WarpTo({0}, {1})", x, y);
    }

    public void TryMove (Directions d, int velocity) {
        Console.WriteLine("Shockwave.TryMove({0}, {1})", d, velocity);
    }
}