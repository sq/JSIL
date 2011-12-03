using System;
using JSIL.Meta;

public static class Program {
    [JSChangeName("RenamedProperty")]
    public static string Property {
        get {
            return "Property";
        }
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Property);
    }
}