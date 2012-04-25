using System;

public struct CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType[5];

        for (var i = 0; i < a.Length; i++)
            Console.WriteLine(a[i].Value);
    }
}