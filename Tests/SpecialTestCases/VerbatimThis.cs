using System;
using JSIL;

public static class Program {
    public class CustomType {
        public CustomType () {
            Console.WriteLine("{0}", Builtins.This);
        }
    }

    public static void Main (string[] args) {
        new CustomType();
    }
}