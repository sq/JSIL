using System;
using JSIL.Proxy;

public static class Program {
    public static int Field1 = 1;
  
    public static void Main (string[] args) {
        Console.WriteLine(Program.Field1);
    }
}

[JSProxy(typeof(Program))]
public static class ProgramProxy {
    public static int Field2;
}