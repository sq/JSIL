using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        object a = instance.A;
        object b = instance.B;
        Console.WriteLine("A = {0}, B = {1}", a, b);
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