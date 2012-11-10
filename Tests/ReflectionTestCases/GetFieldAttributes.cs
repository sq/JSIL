using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(T).GetField("FieldA"), false);
        Common.Util.ListAttributes(typeof(T).GetField("FieldB"), false);
    }
}

public class T {
    [Common.AttributeA]
    public static int FieldA;
    [Common.AttributeA, Common.AttributeB(1, "a")]
    public int FieldB;
}