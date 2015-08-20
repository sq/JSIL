using System;
using JSIL.Meta;

public static class Program {
    [JSReplacement("$typeof(this)")]
    public static Type GetProgramType () {
      return typeof(Program);
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(GetProgramType().Name);
    }
}