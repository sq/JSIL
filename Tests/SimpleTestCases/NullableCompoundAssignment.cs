using System;

public static class Program {
    public static void Main (string[] args) {
        var o = new Obj {
            Prop = Enm.A
        };

        o.Prop |= Enm.C;
    }
}

[Flags]
public enum Enm {
    A = 1,
    B = 2,
    C = 64,
    D = 128
}

public class Obj {
    public Enm? Prop { get; set; }
}