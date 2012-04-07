using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        instance.A<string>();
        instance.B<int>(5);
    }
}

public class CustomType {
    public void A<T> () {
        Console.WriteLine("a<{0}>", typeof(T));
    }
    
    public void B<T> (T value) {
        Console.WriteLine("b<{0}>({1})", typeof(T), value);
    }
}