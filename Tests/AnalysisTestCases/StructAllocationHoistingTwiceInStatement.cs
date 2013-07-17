using System;

public static class Program {
    public static void Main (string[] args) {
        const int numIterations = 6400;

        for (var i = 0; i < numIterations; i++)
            PrintValues(new TestStruct(i), new TestStruct(i * 2));
    }

    public static void PrintValues (TestStruct a, TestStruct b) {
        if ((a.Value % 2000) == 0) {
            Console.WriteLine(a.Value);
            Console.WriteLine(b.Value);
        }
    }
}

public struct TestStruct {
    public readonly int Value;

    public TestStruct (int value) {
        Value = value;
    }
}