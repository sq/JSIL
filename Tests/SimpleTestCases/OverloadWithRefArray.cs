using System;

public static class Program {
    public static void Main (string[] args) {
        var bytes = new byte[] { 0 };

        OverloadedMethod(bytes);
        OverloadedMethod(ref bytes);
    }

    public static void OverloadedMethod (ref byte[] bytes) {
        Console.WriteLine("OverloadedMethod(<ref byte[{0}]>)", bytes.Length);
    }

    public static void OverloadedMethod (byte[] bytes) {
        Console.WriteLine("OverloadedMethod(<byte[{0}]>)", bytes.Length);
    }
}