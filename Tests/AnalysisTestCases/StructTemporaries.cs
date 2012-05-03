using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new TestStruct();
        a.Value = 1;
        var b = a;

        b.Value = 0;

        Console.WriteLine("a = {0}", a.Value);
    }
}

public struct TestStruct {
    public int Value;
}