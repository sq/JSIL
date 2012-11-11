using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(SC1), false);
        Common.Util.ListAttributes(typeof(SC2), false);
    }
}

[Common.AttributeA]
public static class SC1 {
}

[Common.AttributeA, Common.AttributeB(1, "a")]
public static class SC2 {
}