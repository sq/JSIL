using System;

public interface GenericInterface<T> {
    T Method ();
}

public class MyClass : GenericInterface<int>, GenericInterface<string> {
    int GenericInterface<int>.Method () {
        return 1;
    }

    string GenericInterface<string>.Method () {
        return "a";
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = (new MyClass());
        Console.WriteLine(((GenericInterface<int>)a).Method());
        Console.WriteLine(((GenericInterface<string>)a).Method());
    }
}