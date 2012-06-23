using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {

      var list = new List<string> { "zero", "one", "two", "three" };

      Console.WriteLine(list.IndexOf("two"));
      Console.WriteLine(list.IndexOf("zero"));
      Console.WriteLine(list.IndexOf("two-shouldNotExits"));
      Console.WriteLine(list.IndexOf("three"));

      // Other overloads not implemented yet
    }

}