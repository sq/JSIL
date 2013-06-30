using System;
using System.Collections.Generic;

public static class Program {
    public static STex[] list = null;

    public static void Main (string[] args) {
        SplitGrid(null, 4, 4, new Rectangle(0, 0, 256, 256));
        Console.WriteLine(list[0]);
        Console.WriteLine(list[1]);
        Console.WriteLine(list[2]);
    }

    public static void SplitGrid (object tex, int gridx, int gridy, Rectangle sheetrect) {
        bool gappy = false;
        if (gridx < 0) {
            gridx = -gridx;
            gridy = -gridy;
            gappy = true;
        }

        list = new STex[gridx * gridy];

        int cellwidth = sheetrect.Width / gridx;
        int cellheight = sheetrect.Height / gridy;

        for (int y = 0; y < gridy; y++) {
            for (int x = 0; x < gridx; x++) {
                int sheetx = sheetrect.X + x * cellwidth;
                int sheety = sheetrect.Y + y * cellheight;
                Rectangle rect = new Rectangle(sheetx, sheety, cellwidth, cellheight);
                if (gappy) {
                    rect.X += 1;
                    rect.Y += 1;
                    rect.Width -= 2;
                    rect.Height -= 2;
                }
                list[y * gridx + x] = new STex(tex, rect);
            }
        }
    }
}

public struct Rectangle {
    public int X, Y, Width, Height;

    public Rectangle (int x, int y, int width, int height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToString () {
        return String.Format("{0}, {1} w={2} h={3}", X, Y, Width, Height);
    }
}

public struct STex {
    private object tex;
    private Rectangle? srcrect;

    public object Tex
    {
        get { return tex; }
    }
    public Rectangle? Rect
    {
        get { return srcrect; }
    }

    public STex(object texture)
    {
        tex = texture;
        srcrect = null;
    }

    public STex(object texture, Rectangle srcrect)
    {
        tex = texture;
        this.srcrect = srcrect;
    }

    public override string ToString () {
        if (srcrect == null)
            return "null";
        else
            return srcrect.Value.ToString();
    }
}