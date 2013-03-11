using System;

public class Foo<T> where T : Foo<T> {
}

public class Bar : Foo<Bar> {
}

public static class Program {
    public static void Main (string[] args) {
        var b = new Bar();
        Console.WriteLine(b);
        var f = b as Foo<Bar>;
        Console.WriteLine(f);
    }
}