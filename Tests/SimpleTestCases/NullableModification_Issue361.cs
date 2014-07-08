using System;

public static class Program
{
    public static void Main(string[] args)
    {
        MutableStruct? b = new MutableStruct();
        Console.WriteLine(b.Value.Value);
        b.Value.Increment();
        Console.WriteLine(b.Value.Value);
    }
}

public struct MutableStruct
{
    public int Value;

    public void Increment()
    {
        Value++;
    }
}