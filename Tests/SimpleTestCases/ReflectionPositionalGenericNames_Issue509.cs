using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(typeof(Program).GetMethod("TestMethod").ReturnType.Name);
    }

    public static T TestMethod<T>()
    {
        return default(T);
    }
}