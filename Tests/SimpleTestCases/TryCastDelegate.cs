using System;

public static class Program {
    public delegate void NumberPrinter (int i);

    public static void Main (string[] args) {
        NumberPrinter a = PrintNumber;
        object b = a;

        var c = (b as NumberPrinter);

        a(1);
        c(2);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("PrintNumber({0})", x);
    }
}