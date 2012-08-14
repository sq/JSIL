using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(A.GetB());
        Console.WriteLine(B.GetC());
        Console.WriteLine(C.GetA());
    }

    public static int Double (int x) {
        return x * 2;
    }
}

public static class A {
    public static int a;

    static A () {
        a = Program.Double(1);
    }

    public static int GetB () {
        return B.b;
    }
}

public static class B {
    public static int b;

    static B () {
        b = Program.Double(2);
    }

    public static int GetC () {
        return C.c;
    }
}

public static class C {
    public static int c;

    static C () {
        c = Program.Double(4);
    }

    public static int GetA () {
        return A.a;
    }
}