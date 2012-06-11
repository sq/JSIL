using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234, i2 = 123456;
        double d = 1234.56, d2 = 12.3456;

        Console.WriteLine(String.Format("{0:00.00}, {1:00.00}", i, i2));
        Console.WriteLine(String.Format("{0:0000.0000}, {1:0000.0000}", i, i2));
        Console.WriteLine(String.Format("{0:000000.000000}, {1:000000.000000}", i, i2));

        Console.WriteLine(String.Format("{0:00.00}, {1:00.00}", d, d2));
        Console.WriteLine(String.Format("{0:0000.0000}, {1:0000.0000}", d, d2));
        Console.WriteLine(String.Format("{0:000000.000000}, {1:000000.000000}", d, d2));

        Console.WriteLine(String.Format("{0:##.##}, {1:##.##}", i, i2));
        Console.WriteLine(String.Format("{0:####.####}, {1:####.####}", i, i2));
        Console.WriteLine(String.Format("{0:######.######}, {1:######.######}", i, i2));

        Console.WriteLine(String.Format("{0:##.##}, {1:##.##}", d, d2));
        Console.WriteLine(String.Format("{0:####.####}, {1:####.####}", d, d2));
        Console.WriteLine(String.Format("{0:######.######}, {1:######.######}", d, d2));
    }
}