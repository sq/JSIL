using System;

public static class Program {
    public static void Main (string[] args) {
        var SquareArray = new int[2, 2] { { 1, 2 }, { 3, 4 } };
        var SquareEnumerator = SquareArray.GetEnumerator();
        while (SquareEnumerator.MoveNext())
            Console.WriteLine(SquareEnumerator.Current);
    }
}