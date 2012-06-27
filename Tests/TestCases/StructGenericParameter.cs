using System;

public interface I { }
public struct A : I { }

public static class Program {
    public static void Function<T> (ref T obj)
        where T : I {
        I localObj1 = obj;
        var localObj2 = obj;
        Console.WriteLine(object.ReferenceEquals(localObj1, obj) ? "true" : "false");
        Console.WriteLine(object.ReferenceEquals(localObj2, obj) ? "true" : "false");
    }

    public static void Main (string[] args) {
        var a = new A();
        Function(ref a);
    }
}