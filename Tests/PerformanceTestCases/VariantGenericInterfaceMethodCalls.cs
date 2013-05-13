//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int InnerIterationCount = 4096;
    const int IterationCount = 256;

    public static unsafe void Main () {
        Console.WriteLine("Generic Interface: {0:00000.00}ms", Time(TestGenericInterface));
        Console.WriteLine("Variant Generic Interface: {0:00000.00}ms", Time(TestVariantGenericInterface));
    }

    public static int Time (Func<string> func) {
        var started = Environment.TickCount;

        string result = null;

        for (int i = 0; i < IterationCount; i++)
            result = func();

        Console.WriteLine("{0}", result);

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static string TestGenericInterface () {
        string result = null;
        IDumbWorker<string> obj = new DumbWorkerObject();

        for (int i = 0; i < InnerIterationCount; i++)
            result = obj.DoWork();

        return result;
    }

    public static string TestVariantGenericInterface () {
        object result = null;
        var obj = (IDumbWorker<object>)new DumbWorkerObject();

        for (int i = 0; i < InnerIterationCount; i++)
            result = obj.DoWork();

        return (string)result;
    }
}

public interface IDumbWorker<out T> {
    T DoWork ();
}

public class DumbWorkerObject : IDumbWorker<string> {
    const int CharCount = 128;

    public readonly string Prefix;
    public readonly string Suffix;

    public DumbWorkerObject () {
        Prefix = new string('a', CharCount);
        Suffix = new string('b', CharCount);
    }

    public string DoWork () {
        return Prefix + Suffix;
    }
}