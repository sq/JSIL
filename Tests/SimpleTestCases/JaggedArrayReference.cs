using System;

public static class Program {
    public static void Increment (ref int value) {
        value += 1;
    }

    public static void Main (string[] args) {
        var arr = new int[][] {
            new int[] { 1, 2, 3 }, 
            new int[] { 4, 5 }, 
            new int[] { 6 }
        };

        Increment(ref arr[0][0]);
        Increment(ref arr[1][1]);
        Increment(ref arr[2][0]);

        foreach (var subArray in arr)
            foreach (var item in subArray)
                Console.WriteLine(item);
    }
}