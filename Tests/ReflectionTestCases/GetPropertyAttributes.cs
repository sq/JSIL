using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(T).GetProperty("PropertyA"), false);
        Common.Util.ListAttributes(typeof(T).GetProperty("PropertyB"), false);
    }
}

public class T {
    [Common.AttributeA]
    public static int PropertyA { get; set; }
    [Common.AttributeA, Common.AttributeB(1, "a")]
    public int PropertyB { get; set; }
}