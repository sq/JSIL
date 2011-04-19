using System;

public static class Program {
    public static int CountArguments (params int[] numbers) {
        return numbers.Length;
    }

    public static void SumArgumentsWithHeader (string title, params int[] numbers) {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
            sum += numbers[i];

        Console.WriteLine("Sum of {0} is {1}", title, sum);
    }

    public static void Main (string[] args) {
        Console.WriteLine("count={0}", CountArguments(1, 2, 3));
        SumArgumentsWithHeader("test", 1, 2, 3, 5, 6, 10, -4);
    }
}