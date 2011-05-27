using System;
using JSIL.Meta;

public enum CustomEnum : int {
    A = 1,
    B,
    C = 4,
    D
}

public static class Program { 
    public static void Main (string[] args) {
        Func<CustomEnum> e = () => CustomEnum.B;

        switch (e()) {
            case CustomEnum.A:
                Console.WriteLine("a");
                break;
            case CustomEnum.B:
                Console.WriteLine("b");
                break;
            case CustomEnum.C:
                Console.WriteLine("c");
                break;
            default:
                Console.WriteLine("default");
                break;
        }
    }
}