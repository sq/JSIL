using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new TwiceDerivedClass();
        Console.WriteLine(instance);
    }
}

public class BaseClass {
    public BaseClass () {
    }
}

public class DerivedClass : BaseClass {
    [JSExternal]
    public DerivedClass () {
    }
}

public class TwiceDerivedClass : DerivedClass {
    public TwiceDerivedClass () {
        Console.WriteLine("DerivedClass()");
    }
}