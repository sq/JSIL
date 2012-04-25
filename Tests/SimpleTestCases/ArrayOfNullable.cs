using System;

public static class Program {
    public static void Main(string[] args) {
        int?[] arr = new int?[] { 1, null, 2, null };

        Console.WriteLine(arr.Length);
        Console.WriteLine(arr[0]);
    }
}