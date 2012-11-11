using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(S1), false);
        Common.Util.ListAttributes(typeof(S2), false);
    }
}

[Common.AttributeA]
public struct S1 {
}

[Common.AttributeA, Common.AttributeB(1, "a")]
public struct S2 {
}