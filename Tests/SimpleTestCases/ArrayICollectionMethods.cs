using System;
using System.Collections.Generic;


public static class Program {
    public static void Main(string[] args)
    {
        var array = new[]
        {new TestClass(0), new TestClass(1), new TestClass(2), new TestClass(3), new TestClass(4), new TestClass(5)};

        TestCollectionMethod<TestClass>(array);
        TestCollectionMethod<Object>(array);
    }

    public static void TestCollectionMethod<T>(ICollection<T> input)
    {
        var targetArray = new T[8];

        input.CopyTo(targetArray, 2);

        foreach (var testClass in targetArray)
        {
            if (testClass != null)
                Console.WriteLine(testClass);
            else
            {
                Console.WriteLine("null");
            }
        }
    }
}

public class TestClass
{
    public int Value { get; private set; }

    public TestClass(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}