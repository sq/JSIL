using System;

public class CustomTypeA {
    public static void StaticMethod (object a) {
    }
    public static Action<object> Action = new Action<object>(CustomTypeA.StaticMethod);
    public string[] Field { get; set; }
}

public class CustomTypeA<T> : CustomTypeA {
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomTypeA<int>();
    }
}