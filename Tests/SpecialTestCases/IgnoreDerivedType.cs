using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new DerivedClass();
        Console.WriteLine(instance);
    }
}

[JSIgnore]
public class BaseClass {
}

public class DerivedClass : BaseClass {
}