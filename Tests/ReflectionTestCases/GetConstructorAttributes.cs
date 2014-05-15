using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(T).GetConstructors()[0], false);
    }
}

public class T {
    [Common.AttributeA]
    public T () {
    }
}