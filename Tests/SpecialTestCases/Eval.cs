using System;
using JSIL;

public static class Program {
    public static int Foo () {
      var result = Builtins.Eval("2") ?? 1;
      
      return (int)result;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Foo());
    }
}