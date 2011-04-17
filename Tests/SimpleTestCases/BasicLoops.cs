using System;

public static class Program {
    public static void Main (string[] args) {
        int i;

        for (i = 0; i < 10; i++)
            Console.WriteLine(i);

        while (i < 20)
            Console.WriteLine(i++);
    }
}