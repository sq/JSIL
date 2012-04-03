using System;
using System.Reflection;

public class A {
    public void MethodA () {
    }
}

public class B : A {
    public void MethodB () {
    }
}

public static class Program {
    public static void Main (string[] args) {
        Common.Util.AssertMethods(
            typeof(A),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public,
            "MethodA"
        );
        Common.Util.AssertMethods(
            typeof(B),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public,
            "MethodB"
        );
        Common.Util.AssertMethods(
            typeof(A),
            BindingFlags.Instance | BindingFlags.Public,
            "MethodA"
        );
        Common.Util.AssertMethods(
            typeof(B),
            BindingFlags.Instance | BindingFlags.Public,
            "MethodA", "MethodB"
        );
    }
}