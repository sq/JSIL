using System;

public static class Program {
    public static void Main (string[] args) {
        for (int i = 0; i < 5; i++) {
            switch (i) {
                case 0:
                    Console.WriteLine("zero");
                    break;
                case 1:
                    Console.WriteLine("one");
                    break;
                case 2:
                case 3:
                    Console.WriteLine("two or three");
                    break;
                default:
                    Console.WriteLine("four or more");
                    break;
            }
        }
    }
}