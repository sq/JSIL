using System;
using System.Collections.Generic;

public enum Enum1 {
    Value0 = 0,
    Value1 = 1,
}

public static class Program {
    public static void Main (string[] args) {
      var list = new List<Enum1> { Enum1.Value0, Enum1.Value1, Enum1.Value0, (Enum1)3 };

      Console.WriteLine(list.IndexOf(Enum1.Value0));
      Console.WriteLine(list.IndexOf(Enum1.Value1));
      Console.WriteLine(list.IndexOf((Enum1)2));

      // Other overloads not implemented yet
    }

}