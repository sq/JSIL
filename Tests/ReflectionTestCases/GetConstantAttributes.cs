using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(T).GetField("ConstantA"), false);
        Common.Util.ListAttributes(typeof(T).GetField("ConstantB"), false);
    }
}

public class T {
    [Common.AttributeA]
    public const int ConstantA = 1;
    [Common.AttributeA, Common.AttributeB(1, "a")]
    public const int ConstantB = 2;
}