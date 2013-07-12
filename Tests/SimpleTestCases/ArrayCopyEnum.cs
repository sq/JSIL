using System;

public static class Program {
    static Enm[] Values = new[] { Enm.A, Enm.B, Enm.C, Enm.A, Enm.B, Enm.C };

    public static void Main () {
        var temp = new Enm[3];
        Array.Copy(Values, temp, 3);

        foreach (var e in temp)
            Console.WriteLine(e);
    }
}

public enum Enm {
    A,
    B,
    C
}