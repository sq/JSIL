using System;
using System.Threading;

public static class Program {
    public struct MyStruct {
    }

    public static void Main (string[] args) {
        Monitor.Enter(typeof(MyStruct));
        Console.WriteLine("Inside lock");
        Monitor.Exit(typeof(MyStruct));
        Console.WriteLine("Outside lock");
    }
}