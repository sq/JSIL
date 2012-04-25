using System;

public static class Program {
    public static void Main (string[] args) {
        var instanceOfClass = new DerivedClass();
        // Static constructors of generic types do not run in an order matching that of .NET, so we
        //  just want to ensure that both constructors ran without indirectly triggering the base cctor.
        Console.WriteLine("{0} {1}", DerivedClass.CtorText, DerivedClass.CtorText2);
    }
}

public class BaseClass<T> {
    public static string CtorText = null;

    static BaseClass () {
        CtorText = "Hello Base";
    }
}

public class DerivedClass : BaseClass<int> {
    public static string CtorText2 = null;

    static DerivedClass () {
        CtorText2 = "Hello Derived";
    }
}