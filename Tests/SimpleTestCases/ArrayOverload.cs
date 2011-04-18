using System;

public static class Program {
    public static void Main (string[] args) {
        OverloadedMethod();
        OverloadedMethod(1);
        OverloadedMethod(new [] { 1, 2, 3 });
    }

    public static void OverloadedMethod () {
        Console.WriteLine("OverloadedMethod(<void>)");
    }

    public static void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }

    public static void OverloadedMethod (int[] ia) {
        Console.WriteLine("OverloadedMethod(<int[]>)");
    }
}