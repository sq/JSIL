using System;

public static class Program {
    public static void Main (string[] args) {
        Action<int> a =
            (i) => Console.WriteLine("a({0})", i);

        a(1);
        ReturnValue(a)(2);
        ReturnValue(ReturnValue(a))(3);
    }

    public static T ReturnValue<T> (T x) {
        return x;
    }
}