using System;
using JSIL.Meta;

public static class Program {
    [JSExternal]
    public static string Property {
        get {
            return "Property";
        }
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Property);
    }
}