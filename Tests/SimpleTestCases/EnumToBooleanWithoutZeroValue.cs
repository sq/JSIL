using System;

public enum Enm {
    Value3 = 3,
    Value4 = 4,
}

public static class Program {
    public static void Main (string[] args) {
        Func<Enm> f1 = () => Enm.Value3;
        bool areEqual = (f1() == (Enm)0);
        bool areNotEqual = (f1() != (Enm)0);
        Console.WriteLine("{0} {1}", areEqual ? 1 : 0, areNotEqual ? 1 : 0);
    }
}