using System;

public static class Program {
    public static void Main (string[] args) {
        object a = 1.5f;
        object b = 1;

        Console.WriteLine("{0} {1} {2} {3}", (float)a, (int)(float)a, (int)b, (float)(int)b);
    }
}