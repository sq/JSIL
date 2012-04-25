using System;

enum SimpleEnum {
    Value1,
    Value2,
}

public static class Program {
    public static void Main (string[] args) {
        var test = SimpleEnum.Value1;

        if ((test == SimpleEnum.Value1) || (test == SimpleEnum.Value2))
            Console.WriteLine("1");
    }
}