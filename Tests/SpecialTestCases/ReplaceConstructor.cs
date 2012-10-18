using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new MyClass(1);
        Console.WriteLine(instance);
    }
}

public class MyClass {
    public int A;

    [JSReplacement("('myclass' + $a)")]
    public MyClass (int a) {
        A = a;
    }

    public override string ToString () {
        return A.ToString();
    }
}