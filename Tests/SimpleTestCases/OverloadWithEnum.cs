using System;

public enum CustomEnum {
    A = 1,
    B
}

public static class Program {
    public static void Main (string[] args) {
        OverloadedMethod(CustomEnum.A);
        OverloadedMethod((int)CustomEnum.A);
        OverloadedMethod(1);
    }

    public static void OverloadedMethod (CustomEnum e) {
        Console.WriteLine("OverloadedMethod(<CustomEnum>)");
    }

    public static void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }
}