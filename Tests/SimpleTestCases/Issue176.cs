using System;

public static class Program {
    public static int BugTestCase () {
        int tmp = 5;
        int x1 = 10 + tmp;

        tmp = 10;
        int x2 = 15 + tmp;

        return x1 + x2;
    }

    public static void Main () {
        Console.WriteLine(BugTestCase());
    }
}