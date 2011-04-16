using System;

public class CustomType {
    public int A;

    public CustomType (int a) {
        A = a;
    }

    public void Method () {
        Console.WriteLine("A = {0}", A);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance1 = new CustomType(1);
        var instance2 = new CustomType(2);

        Action a = instance1.Method;
        Action b = instance2.Method;

        a();
        b();
    }
}