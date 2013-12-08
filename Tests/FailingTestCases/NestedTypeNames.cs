using System;

public static class Program {
    public static void Main (string[] args) {
        var objs = new object[] { 
            new NonNestedClass(),
            new NonNestedClass.NestedClass(),
            new Namespace.NonNestedClassInNS(),
            new Namespace.NonNestedClassInNS.NestedClass()
        };

        foreach (var obj in objs)
            Console.WriteLine(obj);
    }
}

public class NonNestedClass {
    public class NestedClass {
    }
}

namespace Namespace {
    public class NonNestedClassInNS {
        public class NestedClass {
        }
    }
}