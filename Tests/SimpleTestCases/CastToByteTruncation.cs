using System;

class Program {
    public static void TestTruncation (int value) {
        var b = (byte)value;
        Console.WriteLine("(byte){0} == {1}", value, b);
    }

    public static void Main () {
        TestTruncation(-1024);
        TestTruncation(-255);
        TestTruncation(-32);
        TestTruncation(1);
        TestTruncation(255);
        TestTruncation(1024);
    }
}
