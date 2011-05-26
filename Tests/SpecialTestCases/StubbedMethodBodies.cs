using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
    }

    public static int A {
        get;
        set;
    }
}

public class T {
    public void B () {
    }

    public int C {
        get;
        set;
    }

    public event Action D;
}