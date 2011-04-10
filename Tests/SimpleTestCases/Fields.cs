using System;

public static class Program {
    public static int A = 1, B;
  
    public static void Main (string[] args) {
        Console.WriteLine("A = {0}, B = {1}", A, B);
        Console.WriteLine(new CustomType());
    }
}

public class CustomType {
    public int A = 1, B;

    public override string ToString () {
        return String.Format("A = {0}, B = {1}", A, B);
    }
}