using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        Test();
    }

    public static void Test () {
        var list = new List<int> { 10, 5 };

        foreach (var mem in list) {
            switch (mem) {
                case 6:
                case 29:
                case 30:
                    Console.WriteLine("A");
                    break;
                case 5:
                case 47:
                case 48:
                    Console.WriteLine("B");
                    break;
                case 70:
                    Console.WriteLine("C");
                    break;
            }
        }
    }
}