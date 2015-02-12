using System;

public class T {
    public float[] values = new float[3];

    public float this[int x] {
        get { return this.values[x]; }
        set { this.values[x] = value; }
    }
}

public static class Program {
    public static void Main (string[] args) {
        var t1 = new T();
        t1[0] = t1[1] = 1.0f;

        Console.WriteLine(t1[0]);
    }
}