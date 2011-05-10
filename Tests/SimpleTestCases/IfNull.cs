using System;

public static class Program {
    public static void Main (string[] args) {
        Func<object> o = () => null;

        if (o() == null)
            Console.WriteLine("null");
        else
            Console.WriteLine("not null");

        if (o() != null)
            Console.WriteLine("not null");
        else
            Console.WriteLine("null");
    }
}