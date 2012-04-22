using System;

public static class Program {
    public static Action Printer = PrintSomeText;

    public static void PrintSomeText () {
        Console.WriteLine("Hello");
    }
  
    public static void Main (string[] args) {
        Printer();
    }
}