using System;

public static class Program {
    public static void Main (string[] args) {
        object os = "a";
        object oi = 1;

        TryStringCast(os);
        TryStringCast(oi);
        TryStringCast("a");
        TryGenericCast<string>("a");
    }

    public static void TryStringCast<T> (T value)
        where T : class 
    {
        string s = value as string;
        Console.WriteLine("{0} as string = {1}", value, s ?? "null");
    }

    public static void TryGenericCast<T> (string value)
        where T : class 
    {
        T t = value as T;
        Console.WriteLine("{0} as T = {1}", value, t);
    }
}