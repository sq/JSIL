using System;
using System.Collections.Generic;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        var batcher = new SpriteBatcher();
        batcher.DrawBatch(0);
    }
}

internal struct A {
    public int Value;
}

internal struct B {
    public int Value;
}

internal class SpriteBatcher {
    private readonly List<A> ListA = new List<A>();
    private readonly List<B> ListB = new List<B>();

    private int ASorter (A lhs, A rhs) {
        return rhs.Value - lhs.Value;
    }

    private int BSorter (B lhs, B rhs) {
        return rhs.Value - lhs.Value;
    }

    public void DrawBatch (int i) {
        if (i == 999) {
            ListA.Sort(ASorter);
            ListB.Sort(BSorter);
        }
    }
}
