//@jsiloption codegenerator.eliminatetemporaries false

using System;

public interface I { }
public struct A : I { }

public static class Program {
    public static void Function<T> (ref T obj)
        where T : I {
        // To be completely correct, both of these assignments should generate a copy, 
        //  even though the locals are never mutated.
        I localObj1 = obj;
        var localObj2 = obj;

        // The copies will ensure that both of these ReferenceEquals calls return false.
        // FIXME: They return true because JSIL is certain that the locals are never modified.
        Console.WriteLine(object.ReferenceEquals(localObj1, obj) ? "true" : "false");
        Console.WriteLine(object.ReferenceEquals(localObj2, obj) ? "true" : "false");
    }

    public static void Main (string[] args) {
        var a = new A();
        Function(ref a);
    }
}