using System;

public static class Program
{
    public static void Main(string[] args)
    {
        TestType('a');
        TestType(false);
        TestType(3d);
        TestType(3f);
        TestType(3m);
        TestType(3);
        TestType(3u);
        TestType(3L);
        TestType(3uL);
        //TestType((short)3);
        //TestType((ushort)3);
        //TestType((sbyte)3);
        //TestType((byte)3);
    }

    public static void TestType(object input)
    {
        Console.WriteLine(input.GetType().Name);
    }
}