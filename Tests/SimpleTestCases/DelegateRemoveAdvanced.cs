using System;

public static class Program {
    public static void Main (string[] args) {
        var ca = new MyClass(1);
        var cb = new MyClass(2);

        Action ap = ca.PrintSelf, bp = cb.PrintSelf, ap2 = ca.PrintSelf, bp2 = cb.PrintSelf;

        CompareDelegates(ap, ap);
        CompareDelegates(ap, bp);
        CompareDelegates(ap, ap2);

        Delegate twiceAp = Delegate.Combine(ap, ap);
        Delegate apPair = Delegate.Combine(ap, ap2);

        CompareDelegates(twiceAp, apPair);

        Delegate onceAp = Delegate.Remove(twiceAp, ap);

        CompareDelegates(onceAp, twiceAp);
        CompareDelegates(onceAp, ap);
    }

    public static void CompareDelegates (Delegate a, Delegate b) {
        Console.WriteLine(a == b ? "true" : "false");
    }
}

public class MyClass {
    public int Value;

    public MyClass (int value) {
        Value = value;
    }

    public void PrintSelf () {
        Console.WriteLine("{0}", Value);
    }
}