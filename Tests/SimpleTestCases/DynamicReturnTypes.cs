using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        Console.WriteLine(instance.A());
        Console.WriteLine(instance.B(5));
    }
}

public class CustomType {
    public string A () {
        return "a";
    }
    
    public int B (int i) {
        return i + 1;
    }
}