using System;
using JSIL.Meta;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    const int SieveTarget = 204800;
    const int IterationCount = 256;

    private static bool[] IsNotPrimeTableBool = new bool[4];
    private static byte[] IsNotPrimeTableByte = new byte[4];
    private static readonly List<int> Results = new List<int>();

    private static int ExpectedPrimeCount;

    public static void Main () {
        Results.Clear();
        SieveBools(SieveTarget, Results);
        ExpectedPrimeCount = Results.Count;
        Console.WriteLine(ExpectedPrimeCount);

        Console.WriteLine("Sieve Bytes: {0:00000.00}ms", Time(TimeSieveBytes));
        Console.WriteLine("Sieve Bools: {0:00000.00}ms", Time(TimeSieveBools));
    }

    public static int Time (Action func) {
        var started = Environment.TickCount;

        for (int i = 0; i < IterationCount; i++)
            func();

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static void TimeSieveBools () {
        Results.Clear();
        SieveBools(SieveTarget, Results);
        if (Results.Count != ExpectedPrimeCount)
            throw new Exception("Failed");
    }

    public static void TimeSieveBytes () {
        Results.Clear();
        SieveBytes(SieveTarget, Results);
        if (Results.Count != ExpectedPrimeCount)
            throw new Exception("Failed");
    }

    public static void SieveBools (int target, List<int> result) {
        if (target > IsNotPrimeTableBool.Length)
            Array.Resize(ref IsNotPrimeTableBool, target);

        Array.Clear(IsNotPrimeTableBool, 0, IsNotPrimeTableBool.Length);

        var isNotPrime = IsNotPrimeTableBool;
        var squareRoot = Math.Sqrt(target);

        result.Add(2);

        for (int candidate = 3; candidate < target; candidate += 2) {
            if (isNotPrime[candidate]) 
                continue;

            if (candidate < squareRoot) {
                for (int multiple = candidate * candidate; multiple < target; multiple += 2 * candidate)
                    isNotPrime[multiple] = true;
            }

            result.Add(candidate);
        }
    }

    public static void SieveBytes (int target, List<int> result) {
        if (target > IsNotPrimeTableByte.Length)
            Array.Resize(ref IsNotPrimeTableByte, target);

        Array.Clear(IsNotPrimeTableByte, 0, IsNotPrimeTableByte.Length);

        var isNotPrime = IsNotPrimeTableByte;
        var squareRoot = Math.Sqrt(target);

        result.Add(2);

        for (int candidate = 3; candidate < target; candidate += 2) {
            if (isNotPrime[candidate] != 0)
                continue;

            if (candidate < squareRoot) {
                for (int multiple = candidate * candidate; multiple < target; multiple += 2 * candidate)
                    isNotPrime[multiple] = 1;
            }

            result.Add(candidate);
        }
    }
}