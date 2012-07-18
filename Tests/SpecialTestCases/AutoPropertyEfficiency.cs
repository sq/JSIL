using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine("a={0} b={1} c={2}", instance.A, instance.B, instance.C);
    }
}

public class CustomType {
    public int A {
        get;
        set;
    }
    public virtual int B {
        get;
        private set;
    }
    public int C {
        get {
            return 1;
        }
    }
}