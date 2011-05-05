using System;

public static class Program {
    public const int A = 2;
  
    public static void Main (string[] args) {
        Console.WriteLine("A = {0}, CustomType = {1}", A, new CustomType());
    }
}

public class CustomType {
    public const int A = 1;

    public override string ToString () {
        return String.Format("A = {0}", A);
    }
}