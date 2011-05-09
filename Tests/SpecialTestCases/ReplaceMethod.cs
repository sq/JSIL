using System;
using JSIL.Meta;

public static class Program {
    [JSReplacement("version()")]
    public static string GetJSVersion () {
      return "none";
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(GetJSVersion());
    }
}