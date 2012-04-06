using System;
using System.Reflection;

public class A {
    public void HiddenMethod () {
    }
}

public class B : A {
    new public void HiddenMethod () {
    }
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Common.Util.AssertMembers<MethodInfo>(
            typeof(A),
            BindingFlags.Instance | BindingFlags.Public,
            "HiddenMethod"
        ));

        Console.WriteLine(Common.Util.AssertMembers<MethodInfo>(
            typeof(B),
            BindingFlags.Instance | BindingFlags.Public,
            "HiddenMethod"
        ));
    }
}