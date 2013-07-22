using System;

public static class Program {
    static Enm[] Values = new[] { Enm.A, Enm.B, Enm.C, Enm.A, Enm.B, Enm.C };

    public static void Main () {
        Array.Clear(Values, 1, 2);

        foreach (var e in Values)
            Console.WriteLine(e);
    }
}

public enum Enm {
    A,
    B,
    C
}