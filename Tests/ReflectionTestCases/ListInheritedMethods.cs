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
        Common.Util.AssertMembers<MethodInfo>(
            typeof(A),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public,
            "MethodA"
        );
        Common.Util.AssertMembers<MethodInfo>(
            typeof(B),
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public,
            "MethodB"
        );
        Common.Util.AssertMembers<MethodInfo>(
            typeof(A),
            BindingFlags.Instance | BindingFlags.Public,
            "MethodA"
        );
        Common.Util.AssertMembers<MethodInfo>(
            typeof(B),
            BindingFlags.Instance | BindingFlags.Public,
            "MethodA", "MethodB"
        );
    }
}