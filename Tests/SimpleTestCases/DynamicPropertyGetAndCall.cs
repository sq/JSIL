using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        Console.WriteLine("A = {0}, B = {1}", instance.A, instance.B);
    }
}

public class CustomType {
    public int A = 1;
    public int B {
        get;
        set;
    }
    
    public CustomType () {
        B = 2;
    }
}