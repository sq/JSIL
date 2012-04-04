using System;
using System.Reflection;

public static class Program {
    public static void MethodA () {
    }

    private static void MethodB () {
    }

    public static void Main (string[] args) {
        Common.Util.ListMembers<MethodInfo>(
            typeof(Program),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public
        );
        Common.Util.ListMembers<MethodInfo>(
            typeof(Program),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic
        );
    }
}