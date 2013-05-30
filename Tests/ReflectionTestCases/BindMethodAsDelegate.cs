using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var instance = new T();
        var t = typeof(T);

        var d1 = (Action)Delegate.CreateDelegate(typeof(Action), instance, t.GetMethod("A", new Type[0]));
        var d2 = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), instance, t.GetMethod("A", new Type[] { typeof (object) }));
        var d3 = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), instance, t.GetMethod("A", new Type[] { typeof(int) }));
        var d4 = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), instance, t.GetMethod("A", new Type[] { typeof(string) }));

        d1();
        d2(null);
        d3(0);
        d4(null);
    }
}

public class T {
    public void A () {
        Console.WriteLine("A()");
    }

    public void A (object o) {
        Console.WriteLine("A(object)");
    }

    public void A (int i) {
        Console.WriteLine("A(int)");
    }

    public void A (string s) {
        Console.WriteLine("A(string)");
    }
}