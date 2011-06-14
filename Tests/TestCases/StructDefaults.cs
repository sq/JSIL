using System;

public struct CustomType {
    public int Value;

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}

public static class Program {
    public static CustomType A;

    public static CustomType B {
        get;
        set;
    }

    public static void Main (string[] args) {
        Console.WriteLine("A = {0}, B = {1}", A, B);
    }
}
