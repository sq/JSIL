using System;

public class MyClass {
    public Action<int> GetPrintNumber () {
        return this.PrintNumber;
    }

    public void PrintNumber (int x) {
        Console.WriteLine("MyClass.PrintNumber({0})", x);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new MyClass();
        Action<int> a = PrintNumber;
        Action<int> b = instance.GetPrintNumber();

        a(1);
        b(2);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("PrintNumber({0})", x);
    }
}