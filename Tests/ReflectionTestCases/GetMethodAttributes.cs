using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(T).GetMethod("MethodA"), false);
        Common.Util.ListAttributes(typeof(T).GetMethod("MethodB"), false);
    }
}

public class T {
    [Common.AttributeA]
    public static void MethodA () {
    }
    [Common.AttributeA, Common.AttributeB(1, "a")]
    public void MethodB () {
    }
}