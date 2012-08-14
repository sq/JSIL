using System;

public static class Program {
    public static void Main (string[] args) {
        CheckTypes(1);
        CheckTypes("b");
        CheckTypes(new MyClass());
        CheckTypes(new MyClassT<int>());
        CheckTypes(new MyClassT<object>());
        CheckTypes(new B<object>());
    }

    public static void CheckTypes (object value) {
        if (value is int) {
            Console.WriteLine("int");
        } else if (value is string) {
            Console.WriteLine("string");
        } else if (value is MyClass) {
            Console.WriteLine("class");
        } else {
            Console.WriteLine("who knows");
        }

        if (value is MyClassT<int>)
            Console.WriteLine("class<int>");
        if (value is MyClassT<object>)
            Console.WriteLine("class<object>");

        if (value is B<object>)
            Console.WriteLine("class<object>");
        if (value is I<object[]>)
            Console.WriteLine("interface<object>");
    }
}

public class MyClass {
}

public class MyClassT<T> {
}

public interface I<T> {
}

public class A<T> : I<T> {
}

public class B<T> : A<T[]> {
}