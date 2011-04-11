using System;
using JSIL.Meta;

public static class Program {
    [JSIgnore]
    public static int Field = 1;
  
    public static void Main (string[] args) {
        Console.WriteLine(Program.Field);
    }
}