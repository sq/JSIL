using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(C1), false);
        Common.Util.ListAttributes(typeof(C2), false);
    }
}

[Common.AttributeA]
public class C1 {
}

[Common.AttributeA, Common.AttributeB(1, "a")]
public class C2 {
}