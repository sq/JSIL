using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(CDerived), false);
        Common.Util.ListAttributes(typeof(CDerived), true);
    }
}

[Common.AttributeA]
public class CBase {
}

[Common.AttributeB(1, "a")]
public class CDerived : CBase {
}