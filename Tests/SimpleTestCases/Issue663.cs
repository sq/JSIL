using System;

public static class Program {
    public static void Main () {
        var tempArray = new int[10];
        for (int j = 0; j < 10; j++) {
            tempArray[j] = 10 - j;
        }

        Array.Sort(tempArray, 0, 10);

        foreach (var i in tempArray)
            Console.WriteLine(i);
    }
}