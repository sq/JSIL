using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        try {
            throw new Exception("a");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }

        try {
            throw new Exception("b");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}