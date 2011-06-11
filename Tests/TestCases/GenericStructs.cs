using System;

public struct GenericStruct<T> {
    T Value;

    public GenericStruct (T value) {
        Value = value;
    }

    public void Method () {
        Console.WriteLine("GenericStruct<{0}>({1}).Method()", typeof(T), Value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        (new GenericStruct<int>(1)).Method();
        (new GenericStruct<string>("a")).Method();
    }
}