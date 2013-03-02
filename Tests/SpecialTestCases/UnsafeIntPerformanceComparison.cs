using System;

public static class Program {
    const int BufferSize = 1024 * 1024;
    const int IterationCount = 8;

    public static int[] Buffer = new int[BufferSize];

    public static void Main () {
        Console.WriteLine("Array indexing: {0:00000.00}ms", Time(TestArrayIndexing));
        Console.WriteLine("Pointers: {0:00000.00}ms", Time(TestPointers));
        Console.WriteLine("Pointer inline access: {0:00000.00}ms", Time(TestInlineAccess));
    }

    public static int Time (Action func) {
        var started = Environment.TickCount;

        for (int i = 0; i < IterationCount; i++)
            func();

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static void TestArrayIndexing () {
        for (int i = 0; i < BufferSize; i++)
            Buffer[i] = (i % 255);
    }

    public static unsafe void TestPointers () {
        fixed (int* pBuffer = Buffer)
            for (int i = 0; i < BufferSize; i++)
                pBuffer[i] = (i % 255);
    }

    public static unsafe void TestInlineAccess () {
        fixed (int* pBuffer = Buffer) {
            JSIL.Verbatim.Expression("var offsetInElements = pBuffer.offsetInBytes >>> pBuffer.shift");
            for (int i = 0; i < BufferSize; i++)
                JSIL.Verbatim.Expression("pBuffer.view[(offsetInElements + i) | 0] = (i % 255) | 0");
        }
    }
}