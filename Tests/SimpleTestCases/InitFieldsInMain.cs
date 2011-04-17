using System;

public static class Program {
    public static int A;
    public static string B;
    public static object C;
  
    public static void Main (string[] args) {
        A = 1;
        B = "hello";
        C = new object();
        Console.WriteLine("A = {0}, B = {1}, C = {2}", A, B, C);
    }
}