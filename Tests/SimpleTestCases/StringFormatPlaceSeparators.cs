using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 1234, i2 = 12345, i3 = 123456;

        Console.WriteLine(String.Format("{0:0,0} {1:0,0} {2:0,0}", i, i2, i3));
        Console.WriteLine(String.Format("{0:00,00} {1:00,00} {2:00,00}", i, i2, i3));
        Console.WriteLine(String.Format("{0:000,000} {1:000,000} {2:000,000}", i, i2, i3));

        Console.WriteLine(String.Format("{0:#,#} {1:#,#} {2:#,#}", i, i2, i3));
        Console.WriteLine(String.Format("{0:##,##} {1:##,##} {2:##,##}", i, i2, i3));
        Console.WriteLine(String.Format("{0:###,###} {1:###,###} {2:###,###}", i, i2, i3));
    }
}