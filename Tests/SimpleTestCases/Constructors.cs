using System;

public static class Program { 
    public static void Main (string[] args) {
        Console.WriteLine(new CustomType());
    }
}

public class CustomType {
    public static int A;
    public int B;
    
    static CustomType () {
        A = 1;
    }
  
    public CustomType () {
        B = 2;
    }
    
    public override string ToString () {
        return String.Format("A = {0}, B = {1}", A, B);
    }
}