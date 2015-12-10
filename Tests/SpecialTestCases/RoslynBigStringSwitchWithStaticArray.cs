//@useroslyn

using System;

public static class Program
{
    private static readonly byte[] StaticReadonlyArray = new byte[] { 
        0x52, 0x49, 70, 70, 0, 0, 0, 0, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20, 
        0, 0, 0, 0x10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
        0, 0, 0, 0, 100, 0x61, 0x74, 0x61, 0, 0, 0, 0
     };

    public static void Main(string[] args)
    {
        var a = GetValue();
        switch (a)
        {
            case "A":
                Console.Write("A");
                break;

            case "B":
                Console.Write("B");
                break;
            case "C":
                Console.Write("C");
                break;
            case "D":
                Console.Write("D");
                break;

            case "E":
                Console.Write("E");
                break;

            case "F":
                Console.Write("F");
                break;

            case "G":
                Console.Write("G");
                break;
        }
    }

    private static string GetValue()
    {
        return "C";
    }
}