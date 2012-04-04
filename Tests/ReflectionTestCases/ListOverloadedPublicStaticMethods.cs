using System;
using System.Reflection;

public static class Program {
    public static void MethodA () {
    }

    public static void MethodA (int x) {
    }

    public static void MethodB () {
    }

    public static void Main (string[] args) {
        Common.Util.ListMembers<MethodInfo>(
            typeof(Program),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public
        );
    }
}