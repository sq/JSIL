using System;
using JSIL.Meta;

public enum CustomEnum {
    A = 1,
    B
}

public static class Program {
    public static void Main (string[] args) {
        OverloadedMethod(CustomEnum.A);
        OverloadedMethod((int)CustomEnum.A);
        OverloadedMethod(1);

        OverloadedMethod2(CustomEnum.A);
        OverloadedMethod2("B");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod (CustomEnum e) {
        Console.WriteLine("OverloadedMethod(<CustomEnum>)");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod2 (CustomEnum e) {
        Console.WriteLine("OverloadedMethod(<CustomEnum>)");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod2 (string s) {
        Console.WriteLine("OverloadedMethod(<string>)");
    }
}