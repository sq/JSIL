using System;
using System.Collections.Generic;

public interface A<T> : ICollection<T> {

}

public interface B<T> : ICollection<T> {

}

public interface C {
    IEnumerable<string> List { get; }
}

public class AImpl<T> : List<T>, A<T> {

}

public class BImpl<T> : AImpl<T>, B<T>, C {
    IEnumerable<string> C.List {
        get {
            return new List<string> { "One", "Two", "Three" };
        }
    }
}

public class Program {
    public static void Main (string[] args) {
        C test = new BImpl<string>();
        foreach (var item in test.List) {
            Console.WriteLine(item);
        }
    }
}