using System;

public static class Program {
    public static void Main () {
        var array = new int[10];
        for (int j = 0; j < 10; j++)
            array[j] = j;

        Array.Copy(array, 3, array, 4, 6);
        for (int j = 0; j < 10; j++)
            Console.WriteLine(array[j]);
    }
}