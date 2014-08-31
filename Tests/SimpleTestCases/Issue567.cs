using System;

public static class Program {
    public static void Main (string[] args) {
        GenericClass<A>.Test();
    }
}

public class GenericClass<T1> {
    public static void Test () {
        GenericClass<GenericClass<T1>>.TestMethod((GenericClass<T1>)null);
    }

    public static void TestMethod (T1 arg) {
        Console.WriteLine(typeof(T1).FullName);
    }

    public static void TestMethod (string a) {

    }
}

public class A {
}