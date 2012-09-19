using System;

public static class Program {
    public static void Main (string[] args) {
        int eight = 8;
        int arraySz = eight * eight;

        var tempArray = new int[arraySz];

        Console.WriteLine("tempArray.Length={0}", tempArray.Length);
    }
}