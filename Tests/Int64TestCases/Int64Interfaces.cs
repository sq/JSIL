using System;

public class Program
{
    interface L
    {
        long Value { get; set; }
        long Getter();
        void Setter(long l);
    }

    class A : L
    {
        public long Value { get; set; }
        public long Getter() { return Value; }
        public void Setter(long l) { Value = l; }
    }

    public static void Main()
    {
        var a = new A();
        a.Value = 1L;
        Console.WriteLine(a.Value);
        a.Setter(2L);
        Console.WriteLine(a.Value);

        L l = a;
        l.Value = 3L;
        Console.WriteLine(l.Getter());
    }
}