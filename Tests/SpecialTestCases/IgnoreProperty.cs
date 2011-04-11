using System;
using JSIL.Meta;

public static class Program {
    [JSIgnore]
    public static int Property {
        get;
        set;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Program.Property);
    }
}