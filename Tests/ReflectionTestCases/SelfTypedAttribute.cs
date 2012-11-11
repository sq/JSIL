using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListAttributes(typeof(SelfTypedAttribute), false);
    }
}

[SelfTypedAttribute]
public class SelfTypedAttribute : Attribute {
}