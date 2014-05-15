using System;

public class Program
{
    public static void Main()
    {
        (typeof(Program)).GetMethod("TestDefaultStruct").Invoke(null, new object[] { null });
        try
        {
            (typeof(Program)).GetMethod("TestDefaultStruct").Invoke(null, new object[] { });
        }
        catch (Exception)
        {
            Console.WriteLine("Expected exception: 0 parameters");
        }
        try
        {
            (typeof(Program)).GetMethod("TestDefaultStruct").Invoke(null, new object[] { null, null });
        }
        catch (Exception)
        {
            Console.WriteLine("Expected exception: 2 parameters");
        }
        (typeof(Program)).GetMethod("TestNoArgs").Invoke(null, null);
        (typeof(Program)).GetMethod("TestNoArgs").Invoke(null, new object[] { });
        try
        {
            (typeof(Program)).GetMethod("TestNoArgs").Invoke(null, new object[] { null });
        }
        catch (Exception)
        {
            Console.WriteLine("Expected exception: NoArg - 1 parameter");
        }
    }

    public static void TestDefaultStruct(int testValue)
    {
        Console.WriteLine("TestDefaultStruct" + testValue);

        if (testValue != null)
        {
            Console.WriteLine("Value not default");
        }
    }

    public static void TestNoArgs()
    {
        Console.WriteLine("TestNoArgs");
    }
}