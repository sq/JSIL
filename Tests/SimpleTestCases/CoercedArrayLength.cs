
using System;
public static class Program
{
    public static void Main()
    {
        string[] arr = new string[0];

        // csc is emitting a bool coercion for arr.Length

        if (arr.Length == 0 || arr[0].Length == 0)
            Console.WriteLine("OK");
    }
}