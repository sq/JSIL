using System;

public struct CustomType {
    public int Field;
    public int Property {
        get;
        set;
    }
}

public static class Program {
    public static CustomType A {
        get;
        set;
    }

    public static void Main (string[] args) {
        Console.WriteLine("A.Field = {0}, A.Property = {1}", A.Field, A.Property);
        var a = A;
        a.Field = 1;
        a.Property = 2;
        Console.WriteLine("A.Field = {0}, A.Property = {1}", A.Field, A.Property);
        A = a;
        Console.WriteLine("A.Field = {0}, A.Property = {1}", A.Field, A.Property);
    }
}
