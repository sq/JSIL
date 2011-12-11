using System;

public static class Program {
    public static void Main (string[] args) {
        for (int i = 0; i < 4; i++) {
            switch (i) {
                case 0:
                    Console.WriteLine("zero");
                    break;
                case 2:
                case 3:
                    Console.WriteLine("two or three");
                    break;
                default:
                    Console.WriteLine("one");
                    break;
            }
        }
    }
}