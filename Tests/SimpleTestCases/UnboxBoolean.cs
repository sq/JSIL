using System;

public static class Program {
    public static object ReturnFalse () {
        return false;
    }

    public static void Main () {
        bool b = (bool)ReturnFalse();

        if (b) {
            Console.WriteLine("Ups, false expecetd");
        } else {
            Console.WriteLine("OK");
        }
    }
}