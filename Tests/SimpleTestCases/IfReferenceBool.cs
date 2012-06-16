using System;

public static class Program {
    public static void Test (ref bool b) {
        if (b) {
            Console.WriteLine("true");
        } else {
            Console.WriteLine("false");
        }
    }

    public static void Main (string[] args) {
        bool f = false, t = true;

        Test(ref f);
        Test(ref t);
    }
}
