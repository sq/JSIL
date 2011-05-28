using System;
using JSIL.Meta;

public static class Program {
    private static int _B;

    public static void Main (string[] args) {
    }

    public static int A {
        get;
        set;
    }

    public static int B {
        get {
            return _B;
        }
        set {
            _B = value;
        }
    }
}

public class T {
    private int _E;

    public void C () {
    }

    public int D {
        get;
        set;
    }

    public int E {
        get {
            return this._E;
        }
        set {
            this._E = value;
        }
    }

    public event Action F;
}