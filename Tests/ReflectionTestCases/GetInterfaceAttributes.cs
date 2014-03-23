using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        foreach (var customAttribute in typeof(IInterface).GetCustomAttributes(false)) {
            Console.WriteLine(customAttribute);
        }
    }

    [MyAttribute1, MyAttribute2]
    public interface IInterface {
    }

    public class MyAttribute1 : Attribute {
    }

    public class MyAttribute2 : Attribute {
    }
}