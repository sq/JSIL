using System;
using JSIL;

public static class Program {
    public class CustomType {
        public int B = 0;

        public void Method (ref int a) {
            a += 1;
            B += a;
        }
    }

    public static void Main (string[] args) {
        var instance = new CustomType();
        int i = 0;

        Console.WriteLine(".B = {0}, i = {1}", instance.B, i);
        instance.Method(ref i);
        Console.WriteLine(".B = {0}, i = {1}", instance.B, i);
        instance.Method(ref i);
        Console.WriteLine(".B = {0}, i = {1}", instance.B, i);
    }
}