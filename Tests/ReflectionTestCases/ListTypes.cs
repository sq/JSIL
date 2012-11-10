using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var thisAsm = Assembly.GetExecutingAssembly();

        Common.Util.ListTypes(thisAsm, @"TestNamespace\..*");
    }
}

namespace TestNamespace {
    public class PublicClass {
    }

    public struct PublicStruct {
    }

    internal class InternalClass {
    }

    internal struct InternalStruct {
    }

    class PrivateClass {
    }

    struct PrivateStruct {
    }
}