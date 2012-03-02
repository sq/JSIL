using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        Console.WriteLine(instance);
        instance.A = 2;
        instance.B = 4;
        Console.WriteLine(instance);
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
    
    public override string ToString () {
        return String.Format("A = {0}, B = {1}", A, B);
    }
}