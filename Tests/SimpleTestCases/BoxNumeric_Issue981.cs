using System;

public static class Program {
    public static void Main (string[] args)
    {
        object v = (byte)3;
        Console.WriteLine(v.GetType().FullName);
        v = (uint)3;
        Console.WriteLine(v.GetType().FullName);
        v = (ushort)3;
        Console.WriteLine(v.GetType().FullName);
        v = (short)3;
        Console.WriteLine(v.GetType().FullName);
    }
}