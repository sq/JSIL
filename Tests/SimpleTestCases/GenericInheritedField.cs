using System;

public class CustomTypeA<T> {
    public string[] Field { get; set; }
}

public class CustomTypeB<T> : CustomTypeA<T> {
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomTypeB<int>();
        Console.WriteLine(instance.Field == null ? "null" : "error");
    }
}