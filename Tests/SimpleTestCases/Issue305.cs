using System;
using JSIL;

class Program {
    public static void Main () {
        object instance = null;
        if (Builtins.IsJavascript) {
            instance = Verbatim.Expression("JSIL.CreateInstanceOfType(TestClass.__Type__)");
        } else {
            instance = new TestClass();
        }
        Console.WriteLine(instance);
    }
}

public class TestClass {
    public TestClass () {
        Console.WriteLine("Constructor");
    }
}