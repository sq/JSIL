using System;
using JSIL.Meta;

public static class Program {
    public static string Property {
        [JSExternal]
        get {
            return "Property";
        }
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Property);
    }
}