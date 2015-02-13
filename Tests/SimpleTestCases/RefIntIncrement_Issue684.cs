using System;

public static class Program {
    public static void Main (string[] args)
    {
        int i = 1;
        Test(ref i);
    }

    public static void Test(ref int i)
    {
        Console.WriteLine(i++);
    }
}

