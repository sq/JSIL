using System;
using System.Collections.Generic;

public static class Program {
    public static object CreateActor (ImmutableCustomType pos) {
        return pos;
    }

    public static void Main (string[] args) {
        var actors = new List<object>();
        int width = 3, height = 3;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var pos = new ImmutableCustomType(x + (y * width));

                actors.Add(CreateActor(pos));
            }
        }

        foreach (var actor in actors)
            Console.Write("{0} ", actor);
        Console.WriteLine();
    }
}

public struct ImmutableCustomType {
    public readonly int Value;

    public ImmutableCustomType (int value) {
        Value = value;
    }

    public ImmutableCustomType Add (int amount) {
        return new ImmutableCustomType(Value + amount);
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}