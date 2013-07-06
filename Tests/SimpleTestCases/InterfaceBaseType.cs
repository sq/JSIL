using System;

interface I {
}

class Program {
    public static void Main () {
        Console.WriteLine(typeof(I).BaseType);
        Console.WriteLine(typeof(I).BaseType == null ? "true" : "false");
  }
}
