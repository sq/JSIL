using System;

public static class Program {
    public static int A {
        get;
        set;
    }
    public static int B {
        get;
        set;
    }
    
    static Program () {
        A = 1;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine("A = {0}, B = {1}", A, B);
        Console.WriteLine(new CustomType());
    }
}

public class CustomType {
    public int A {
        get;
        set;
    }
    public int B {
        get;
        set;
    }
    
    public CustomType () {
        A = 1;
    }
    
    public override string ToString () {
        return String.Format("A = {0}, B = {1}", A, B);
    }
}