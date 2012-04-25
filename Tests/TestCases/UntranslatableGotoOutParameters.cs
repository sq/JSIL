using System;
using JSIL.Meta;

public static class Program {
    public static object func (object o) {
        return null;
    }

    public static bool func2 (out object obj) {
        obj = null;
        return false;
    }

    public static object TestUntranslatableGotoWithOutParam () {
        object ret;
        if (func2(out ret)) {
        }
        return func(ret);
    }
    
    public static void Main (string[] args) {
        Console.WriteLine(": {0}", TestUntranslatableGotoWithOutParam());
    }
}