using System;

public class Program {
    class S { public int s; }

    interface L {
        S Value { get; set; }
        S Getter ();
        void Setter (S l);
    }

    class A : L {
        public S Value { get; set; }
        public S Getter () { return Value; }
        public void Setter (S l) { Value = l; }
    }

    public static void Main () {
        var a = new A();
        a.Value = new S { s = 1 };
        Console.WriteLine(a.Value.s);

        a.Setter(new S { s = 2 });
        Console.WriteLine(a.Value.s);

        L l = a;
        l.Value = new S { s = 3 };
        Console.WriteLine(l.Getter().s);  // wrong (2)
        Console.WriteLine(a.Getter().s);  // wrong (2)
        Console.WriteLine(l.Value.s);     // right (3)
        Console.WriteLine(a.Value.s);     // wrong (2)
    }
}