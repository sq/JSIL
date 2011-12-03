using System;
using JSIL.Meta;

public static class Program {
    [JSExternal]
    public static string Field = "Field";
  
    public static void Main (string[] args) {
        Console.WriteLine(Field);
    }
}