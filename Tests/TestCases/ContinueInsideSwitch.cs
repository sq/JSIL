using System;

public static class Program {
    public static void Main (string[] args) {
        for (int i = 0; i < 3; i++) {
            switch (i) {
                case 0:
                    Console.WriteLine("zero");
                    continue;
                case 1:
                    Console.WriteLine("one");
                    continue;
                case 2:
                    Console.WriteLine("two");
                    break;
            }
            Console.WriteLine("nocontinue");
        }
    }
}