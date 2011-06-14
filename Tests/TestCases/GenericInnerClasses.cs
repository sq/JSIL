using System;

public class GenericClass<T> {
    public class InnerClass {
        public void Method (T value) {
            Console.WriteLine("GenericClass<{0}>.InnerClass.Method({1})", typeof(T), value);
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        (new GenericClass<int>.InnerClass()).Method(1);
        (new GenericClass<string>.InnerClass()).Method("a");
    }
}