using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        Common.Util.ListMembers<MethodInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public
        );
        Common.Util.ListMembers<MethodInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic
        );
        Common.Util.ListMembers<MethodInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
        );
        Common.Util.ListMembers<MethodInfo>(
            typeof(T),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic
        );
    }
}

public class T {
    static T () {
    }

    public static void MethodA () {
    }

    private static void MethodB () {
    }

    public void MethodC () {
    }

    private void MethodD () {
    }
}