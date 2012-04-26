using System;

public class Program {

    interface L {
        int Getter ();
    }

    class A : L {
        public int Getter () { return 21; }
    }

    public static void Main (string[] args) {
        L l = new A();
        Console.WriteLine(l.Getter());
    }
}