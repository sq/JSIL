using System;

public struct CustomType {
    public int Field;
    public int Property {
        get;
        set;
    }

    public override string ToString () {
        return String.Format("<Field={0}, Property={1}>", Field, Property);
    }
}

public class A {
    public CustomType Field1;
}

public class B : A {
    public CustomType Field2;
}

public static class Program {
    public static void Main (string[] args) {
        var test = new B();
        var test2 = test;
        Console.WriteLine("test.Field1 = {0}, test.Field2 = {1}", test.Field1, test.Field2);
        Console.WriteLine("test2.Field1 = {0}, test2.Field2 = {1}", test2.Field1, test2.Field2);
        test.Field1.Field = 1;
        test.Field2.Property = 2;
        Console.WriteLine("test.Field1 = {0}, test.Field2 = {1}", test.Field1, test.Field2);
        Console.WriteLine("test2.Field1 = {0}, test2.Field2 = {1}", test2.Field1, test2.Field2);
    }
}
