
using System;
using System.Runtime.CompilerServices;

public class Program
{
    public static void Main()
    {
        TestAll();
    }

    public static bool ParanoidAssertLongLong(long data, long _int, string should,
       string fn = "?", string fun = "?",
       int ln = -1)
    {
        var ds = data.ToString();
        if (ds != should || data != _int)
        {
            Console.WriteLine("got {0}, expected {1}, from {2}:{3}", data, should, fun, ln);
            return false;
        }
        else
        {
            Console.WriteLine("OK");
            return true;
        }
    }

    public static void TestAll()
    {
        TestLsLong(0x0102030405060708, 8, 144964032628459520, "144964032628459520");
        TestRsLong(0x0102030405060708, 8, 283686952306183, "283686952306183");
        unchecked
        {
            TestLsLong((long)0xff02030405060708, 8, 144964032628459520, "144964032628459520");
            TestRsLong((long)0xff02030405060708, 8, -279263001115129, "-279263001115129");
            TestLsLong((long)0xffff030405060708, 8, -71208749485324288, "-71208749485324288");
            TestRsLong((long)0xffff030405060708, 8, -1086559287801, "-1086559287801");
            TestXorLongLong(0x0102030405060708, 0x1020304050607080, 1234605616436508552, "1234605616436508552");
            TestAndLongLong(0x0102030405060708, 0x1020304050607080, 0, "0");
            TestAndLongLong(0x0102030405060708, (long)0xff00ff00ff00ff00, 72060892656699136, "72060892656699136");
            TestCastLongByte(0x0102030405060708, 8, "8");
            TestCastLongByte((long)0xff02030405060708, 8, "8");
            TestCastLongByte(0x01020304050607ff, 255, "255");
            TestCastLongByte(-1, 255, "255");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void TestCastLongByte(long p1, long p3, string p4, [CallerFilePath] string fn = "?", [CallerMemberName]string fun = "?",
[CallerLineNumber] int ln = -1)
    {
        var t = (byte)(p1);
        ParanoidAssertLongLong(t, p3, p4, fn, fun, ln);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void TestAndLongLong(long p1, long p2, long p3, string p4, [CallerFilePath] string fn = "?", [CallerMemberName]string fun = "?",
[CallerLineNumber] int ln = -1)
    {
        var t = (long)(p1 & p2);
        ParanoidAssertLongLong(t, p3, p4, fn, fun, ln);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void TestXorLongLong(long p1, long p2, long p3, string p4, [CallerFilePath] string fn = "?", [CallerMemberName]string fun = "?",
[CallerLineNumber] int ln = -1)
    {
        var t = (long)(p1 ^ p2);
        ParanoidAssertLongLong(t, p3, p4, fn, fun, ln);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void TestRsLong(long p1, int p2, long p3, string p4, [CallerFilePath] string fn = "?", [CallerMemberName]string fun = "?",
[CallerLineNumber] int ln = -1)
    {
        var t = (long)(p1 >> p2);
        ParanoidAssertLongLong(t, p3, p4, fn, fun, ln);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void TestLsLong(long p1, int p2, long p3, string p4, [CallerFilePath] string fn = "?", [CallerMemberName]string fun = "?",
[CallerLineNumber] int ln = -1)
    {
        var t = (long)(p1 << p2);
        ParanoidAssertLongLong(t, p3, p4, fn, fun, ln);
    }

}