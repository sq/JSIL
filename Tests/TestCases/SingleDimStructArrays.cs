using System;

public struct Struct {
    public int I;

    public override string ToString () {
        return I.ToString();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new Struct[10];

        for (var i = 0; i < a.Length; i++)
            a[i] = new Struct {
                I = i
            };

        foreach (var s in a)
            Console.WriteLine(s);
    }
}