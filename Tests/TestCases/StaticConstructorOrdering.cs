using System;

public static class Program {
    public static void Main (string[] args) {
        var clas = new SomeDerivedClass();
    }
}

public class SomeBaseClass {
    static SomeBaseClass () {
        Console.WriteLine("SomeBaseClass");
    }
}

public class SomeDerivedClass : SomeBaseClass {
    static SomeDerivedClass () {
        Console.WriteLine("SomeDerivedClass");
    }
}