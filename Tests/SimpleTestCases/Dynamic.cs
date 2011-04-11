using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        instance.A();
        instance.B(5);
    }
}

public class CustomType {
    public void A () {
        Console.WriteLine("a");
    }
    
    public void B (int i) {
        Console.WriteLine("b {0}", i + 1);
    }
}