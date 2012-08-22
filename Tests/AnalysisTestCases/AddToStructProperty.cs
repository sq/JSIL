using System;
using System.Collections.Generic;

public static class Program {
    public const int ShockwaveVelocity = 384;
    public const int KinematicObstructionGridSize = 8;

    public static void Main (string[] args) {
        var objects = new List<Obj> {
            new Obj(), new Obj(), new Obj()
        };

        for (int i = 0; i < objects.Count; i++) {
            Str d = new Str { Value = i };
            objects[i].Property += d;
        }

        foreach (var o in objects)
            Console.WriteLine(o.Property.Value);
    }
}

public class Obj {
    private int p;

    public Str Property {
        get {
            return new Str { Value = p };
        }
        set {
            p = value.Value;
        }
    }
}

public struct Str {
    public int Value;

    public static Str operator + (Str lhs, Str rhs) {
        return new Str {
            Value = lhs.Value + rhs.Value
        };
    }
}