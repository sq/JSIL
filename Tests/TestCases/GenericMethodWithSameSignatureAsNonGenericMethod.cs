using System;

public static class Program {
    public static void Test<T> () {
        Console.WriteLine("Generic");
    }

    public static void Test (Type type) {
        Console.WriteLine("Parameter");
    }

    public static void Main (string[] args) {
        Test<object>();
        Test(typeof(object));
        Console.WriteLine("Done");
    }
}
