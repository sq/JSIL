using System;
using JSIL.Meta;

public static class Program {
    [JSIgnore]
    public class CustomType {
    }
  
    public static void Main (string[] args) {
        object o1 = "a";
        Method(o1);
        object o2 = new CustomType();
        Method(o2);
    }

    public static void Method (object o) {
        Console.WriteLine("Method(<object>)");
    }

    public static void Method (CustomType ct) {
        Console.WriteLine("Method(<CustomType>)");
    }
}