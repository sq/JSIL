using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine("A = {0}", instance.A);
        Console.WriteLine("A++ = {0}", instance.PostIncrementA());
        Console.WriteLine("++A = {0}", instance.PreIncrementA());
    }
}

public abstract class CustomTypeBase {
    public int A { get; set; }
}

public class CustomType : CustomTypeBase {
    public int PreIncrementA () {
        return ++base.A;
    }

    public int PostIncrementA () {
        return base.A++;
    }
}