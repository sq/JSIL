using System;

public static class Program { 
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        instance.OverloadedMethod();
        instance.OverloadedMethod(3);
        instance.OverloadedMethod("a");
    }
}

public class CustomType {
    public void OverloadedMethod () {
        Console.WriteLine("OverloadedMethod(<void>)");
    }

    public void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }

    public void OverloadedMethod (string s) {
        Console.WriteLine("OverloadedMethod(<string>)");
    }
}