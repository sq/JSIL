using System;

public class GenericClass<T> {
    public GenericStruct<T> Field;
}

public struct GenericStruct<T> {
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new GenericClass<string>().Field);
        Console.WriteLine(new GenericClass<int>().Field);
    }
}