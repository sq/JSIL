using System;

public static class Program {
    public static void Main (string[] args) {
        dynamic instance = new CustomType();
        var a = (int)instance.A;
        var b = int.Parse((string)instance.B);
        Console.WriteLine("A = {0}, B * 2 = {1}", a, b * 2);
    }
}

public class CustomType {
    public int A = 1;
    public string B = "2";
}