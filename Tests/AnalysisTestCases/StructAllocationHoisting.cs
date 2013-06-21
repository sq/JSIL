using System;

public static class Program {
    public static void Main (string[] args) {
        const int numIterations = 10240;

        for (var i = 0; i < numIterations; i++)
            PrintValue(new TestStruct(i));
    }

    public static void PrintValue (TestStruct s) {
        if ((s.Value % 2000) == 0)
            Console.WriteLine(s.Value);
    }
}

public struct TestStruct {
    public readonly int Value;

    public TestStruct (int value) {
        Value = value;
    }
}