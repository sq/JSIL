using System;

public class CustomType {
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new object());
        Console.WriteLine(new CustomType());
    }
}