//@compileroption /unsafe

using System;
using System.Collections.Generic;
using JSIL.Meta;

public static class Program {
    const int AxisSize = 20;
    const int BufferSize = (AxisSize * AxisSize);
    const int IterationCount = 8192;

    public static List<int> Results = new List<int>();

    public static Rectangle[] Rectangles = new Rectangle[BufferSize];
    [JSPackedArray]
    public static Rectangle[] PackedRectangles = new Rectangle[BufferSize];

    public static unsafe void Main () {
        for (var y = 0; y < AxisSize; y++) {
            for (var x = 0; x < AxisSize; x++) {
                var index = x + (y * AxisSize);

                Rectangles[index] = new Rectangle(x * 32, y * 32, 32, 32);
                PackedRectangles[index] = Rectangles[index];
            }
        }

        Console.WriteLine("Intersect: {0:00000.00}ms", Time(TestIntersect));
        Console.WriteLine("Intersect Packed: {0:00000.00}ms", Time(TestIntersectPacked));
    }

    public static int Time (Action func) {
        var started = Environment.TickCount;

        for (int i = 0; i < IterationCount; i++) {
            func();
        }

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static void TestIntersect () {
        Results.Clear();
        var testRect = new Rectangle(16, 16, 32, 32);

        for (var i = 0; i < BufferSize; i++) {
            if (testRect.Intersects(ref Rectangles[i]))
                Results.Add(i);
        }
    }

    public static void TestIntersectPacked () {
        Results.Clear();
        var testRect = new Rectangle(16, 16, 32, 32);

        for (var i = 0; i < BufferSize; i++) {
            if (testRect.Intersects(ref PackedRectangles[i]))
                Results.Add(i);
        }
    }
}

public struct Rectangle {
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;

    public Rectangle (int x, int y, int width, int height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Intersects (ref Rectangle value) {
        return (value.X < this.X + this.Width) && 
            (this.X < value.X + value.Width) && 
            (value.Y < this.Y + this.Height) && 
            (this.Y < value.Y + value.Height);
    }
}