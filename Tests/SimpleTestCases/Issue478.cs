using System;

public static class Program {
    public static void Main(string[] args) {
        object obj = "string";
        obj = Hello.One;
        Console.WriteLine((int)obj); //Output: 52
        Console.WriteLine((System.Int32)obj); //Output : 52

        try {
            Console.WriteLine((float)obj);
        } catch (System.InvalidCastException) {
            Console.WriteLine("invalid cast exception (float)");
        }

        try {
            Console.WriteLine((bool)obj);
        }
        catch (System.InvalidCastException) {
            Console.WriteLine("invalid cast exception (bool)");
        }

        try {
            Console.WriteLine((string)obj);
        }
        catch (System.InvalidCastException) {
            Console.WriteLine("invalid cast exception (string)");
        }

        try {
            Console.WriteLine((System.Int64)obj);
        }
        catch (System.InvalidCastException) {
            Console.WriteLine("invalid cast exception (System.Int64)");
        }
    }

    public enum Hello {
        One = 52,
        Two
    }
}