using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var a = (object)new Implementor2<object, string>();
        var b = (IInterface<string>)a;
        b.Get();
        var c = (IInterface<object>)a;
        c.Get();
    }
}

public interface IInterface<out T> {
    T Get ();
}

public class Implementor<T1> : IInterface<T1> {
    T1 IInterface<T1>.Get () {
        Console.WriteLine("Implementor<" + typeof(T1).Name + ">.Get");
        return default(T1);
    }
}

public class Implementor2<T1, T2> : Implementor<T1>, IInterface<T2> {
    T2 IInterface<T2>.Get () {
        Console.WriteLine("Implementor2<" + typeof(T1).Name + "," + typeof(T2).Name + ">.Get");
        return default(T2);
    }
}