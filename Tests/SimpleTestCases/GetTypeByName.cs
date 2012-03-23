using System;

public class CustomType {
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(System.Type.GetType("CustomType"));
        Console.WriteLine(System.Type.GetType("System.String"));
    }
}