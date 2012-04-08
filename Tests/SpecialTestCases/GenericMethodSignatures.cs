using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static T GenericMethod<T> (T value) {
        return value;
    }

    public static void PrintValues<T> (T[] values) {
        foreach (var value in values)
            Console.WriteLine(value);
    }

    public static void PrintValues<T> (IEnumerable<T> values) {
        foreach (var value in values)
            Console.WriteLine(value);
    }

    public static void Main (string[] args) {
        PrintValues(new[] { GenericMethod(1) });
    }
}

public static class GenericStatic<T> {
    public static T NormalMethod (T value) {
        return value;
    }

    public static T GenericMethod<U> (T value, U value2) {
        return value;
    }
}

public class GenericInstance<T> {
    public T NormalMethod (T value) {
        return value;
    }

    public T GenericMethod<U> (T value, U value2) {
        return value;
    }
}