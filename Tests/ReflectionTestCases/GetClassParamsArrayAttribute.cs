using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(C1), false);
    }
}

public class ArrayAttribute : Attribute {
    public readonly object[] Arr;

    public ArrayAttribute (params object[] arr) {
        Arr = arr;
    }

    public override string ToString () {
        return String.Format("ArrayAttribute({0})", Arr);
    }
}

[ArrayAttribute(1, 2, "a")]
public class C1 {
}