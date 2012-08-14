using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var o = new Obj();
        Console.WriteLine(o.Value);
    }

    public static int Double (int x) {
        return x * 2;
    }
}

public class Obj : Base {
    public static int StaticValue;
    public int Value;

    static Obj () {
        StaticValue = Program.Double(2);
    }

    public Obj () 
        : base () {
        Value = Program.Double(StaticValue);
    }
}

public class Base {
    public Base () {
        Console.WriteLine(Obj.StaticValue);
    }
}
