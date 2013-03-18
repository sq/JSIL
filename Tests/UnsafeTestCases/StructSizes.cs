using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var types = new Type[] {
            typeof(Struct1),
            typeof(Struct2),
            typeof(Struct3),
            typeof(Struct4),
            typeof(Struct5),
            typeof(Struct6)
        };

        foreach (var type in types) {
            Console.WriteLine("sizeof({0}) == {1}", type.Name, Marshal.SizeOf(type));
        }
    }
}

public struct Struct1 {
}

public struct Struct2 {
    byte a, b;
}

public struct Struct3 {
    byte a, b;
    int c;
}

public struct Struct4 {
    byte a, b;
    short c;
    double d;
}

public struct Struct5 {
    double a;
    byte b, c;
}

public struct Struct6 {
    byte a;
    Struct5 b;
    byte c;
}