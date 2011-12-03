using System;
using JSIL.Meta;

public static class Program {
    [JSExternal]
    public static void Method () {
        Console.WriteLine("Method");
    }
  
    public static void Main (string[] args) {
        Method();
    }
}