using System;
using JSIL.Meta;

public static class Program {
    [JSIgnore]
    public class TestClass {
    }
  
    public static void Main (string[] args) {
        TestClass local = null;
        Console.WriteLine(local);
    }
}