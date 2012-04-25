using System;

public static class Program {
    public static void Main (string[] args) {
        var array = new int[] { 0, 1, 2, 3 };
        var index = (char)1;

        Console.WriteLine("{0} {1} {2}", array['d' - 'd'], array[index], array['c' - 'a']);
        array[0] = 5;
        array[index] = 6;
        array['c' - 'a'] = 4;
        Console.WriteLine("{0} {1} {2}", array['d' - 'd'], array[index], array['c' - 'a']);
    }
}