using System;

public static class Program {
    public static void Main (string[] args) {
        var baseType = typeof(object).BaseType;

        if (baseType == null)
            Console.WriteLine("null");
        else
            Console.WriteLine(baseType.Name);
    }
}