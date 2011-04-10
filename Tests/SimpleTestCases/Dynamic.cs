using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        instance.A();
        instance.B();
    }
}

public class CustomType {
    public void A () {
        Console.WriteLine("a");
    }
    
    public void B () {
        Console.WriteLine("b");
    }
}