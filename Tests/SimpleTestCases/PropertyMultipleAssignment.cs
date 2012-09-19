using System;

public static class Program {
    static int a, b, c;

    public static int A {
        get {
            return a;
        }
        set {
            a = value;
        }
    }

    public static int B {
        get {
            return b;
        }
        set {
            b = value;
        }
    }

    public static int C {
        get {
            return c;
        }
        set {
            c = value;
        }
    }

    public static void Main (string[] args) {
        A = B = C = 1;
        Console.WriteLine("A={0}, B={1}, C={2}", A, B, C);
        C = A = 2;
        Console.WriteLine("A={0}, B={1}, C={2}", A, B, C);
    }
}