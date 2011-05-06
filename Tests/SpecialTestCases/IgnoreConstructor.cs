using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        new Test(1);
        new Test("a");
    }
}

public class Test {
    [JSIgnore]
    public Test (string s) {
        Console.WriteLine("new Test(<string>)");
    }

    public Test (int a) {
        Console.WriteLine("new Test(<int>)");
    }
}