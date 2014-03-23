using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        foreach (var customAttribute in typeof(this).GetMethod("Method").GetCustomAttributes(false)) {
            Console.WriteLine(customAttribute);
        }
    }

    public static void Method (
        [MyAttribute1]
        int i,
        [MyAttribute2]
        float f
    ) {
    }
}

public class MyAttribute1 : Attribute {
}

public class MyAttribute2 : Attribute {
}
