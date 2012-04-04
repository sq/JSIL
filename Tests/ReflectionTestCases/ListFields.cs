using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListMembers<FieldInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public
        );
        Common.Util.ListMembers<FieldInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic
        );
        Common.Util.ListMembers<FieldInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
        );
        Common.Util.ListMembers<FieldInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic
        );
    }
}

public class T {
    public static int FieldA;
    public int FieldB;
    private static int FieldC;
    private int FieldD;
}