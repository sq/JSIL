using System;
using JSIL.Meta;

public static class Program {
    [JSChangeName("RenamedMethod")]
    public static void Method () {
        Console.WriteLine("Method");
    }
  
    public static void Main (string[] args) {
        Method();
    }
}