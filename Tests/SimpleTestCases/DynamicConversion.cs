using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        var a = (int)instance.A;
        Console.WriteLine("A = {0}", a);
    }
}

public class CustomType {
    public int A = 1;
}