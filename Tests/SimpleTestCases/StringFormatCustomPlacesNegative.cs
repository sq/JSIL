using System;

public static class Program {
    public static void Main (string[] args) {
        int i = -1234, i2 = -123456;

        Console.WriteLine(String.Format("{0:00}, {1:00}", i, i2));
        Console.WriteLine(String.Format("{0:0000}, {1:0000}", i, i2));
        Console.WriteLine(String.Format("{0:000000}, {1:000000}", i, i2));

        Console.WriteLine(String.Format("{0:##}, {1:##}", i, i2));
        Console.WriteLine(String.Format("{0:####}, {1:####}", i, i2));
        Console.WriteLine(String.Format("{0:######}, {1:######}", i, i2));

        Console.WriteLine(String.Format("{0:000.000}", -1.337f));
    }
}