using System;
using JSIL.Meta;

public static class Program {
    public static int DBF = 3;

    internal static void ApplyState () {
        if (false) {
        } else {
            Console.WriteLine("Entered");

            int func;

            switch (DBF) {
                default:
                case 0:
                    Console.WriteLine("Default case");
                    func = 0;
                    break;
                case 1:
                    Console.WriteLine("case 1");
                    func = 1;
                    break;
                case 2:
                    Console.WriteLine("case 2");
                    func = 2;
                    break;
                case 3:
                    Console.WriteLine("case 3");
                    func = 3;
                    break;
            }

            Console.WriteLine("Tail");
        }

        Console.WriteLine("Exited");
    }

    public static void Main (string[] args) {
        DBF = 0;
        ApplyState();
        DBF = 2;
        ApplyState();
        DBF = 4;
        ApplyState();
    }
}