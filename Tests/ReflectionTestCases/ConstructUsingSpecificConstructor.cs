using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var t = typeof(T);

        var c1 = t.GetConstructor(new Type[0]);
        var c2 = t.GetConstructor(new Type[] { typeof(int) });
        var c3 = t.GetConstructor(new Type[] { typeof(string) });

        var instance1 = c1.Invoke(new object[0]);
        int i = 5;
        var instance2 = c2.Invoke(new object[] { i });
        string s = "a";
        var instance3 = c3.Invoke(new object[] { s });

        Console.WriteLine("{0} {1} {2}", instance1, instance2, instance3);
    }
}

public class T {
    public T () {
        Console.WriteLine("new T()");
    }

    public T (int i) {
        Console.WriteLine("new T(int)");
    }

    public T (string s) {
        Console.WriteLine("new T(string)");
    }
}