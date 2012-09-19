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
        Print(a.Value);
        a.Setter(2L);
        Print(a.Value);

        L l = a;
        l.Value = 3L;
        Print(l.Getter());
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}