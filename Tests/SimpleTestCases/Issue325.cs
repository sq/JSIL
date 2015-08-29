using System;

public static class Program {
    public static void Main(string[] args) {
        long i1 = 1;
        long i2 = 1;
        System.Console.WriteLine(i1.GetHashCode() == i2.GetHashCode() ? "true" : "false"); // .NET prints an empty line, JSIL prints "null"
    }
}