using System;
using JSIL;

public static class Program {
    public static int Foo () {
      Verbatim.Eval(@"return 2");
      
      return 1;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine(Foo());
    }
}