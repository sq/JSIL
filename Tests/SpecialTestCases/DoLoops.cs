using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        int i = 0;

        do {
            Console.WriteLine(i);
            i += 1;
        } while (i < 5);

        do {
            i--;
        } while (i > 1);

        Console.WriteLine(i);
    }
}