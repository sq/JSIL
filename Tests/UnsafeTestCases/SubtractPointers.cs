using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var ints = new int[] { 0, 1, 2, 3, 4, 5 };

        fixed (int* p0 = &ints[0], p1 = &ints[1], p5 = &ints[5]) {
            Console.WriteLine((int)(p1 - p0));
            Console.WriteLine((int)(p5 - p0));
            Console.WriteLine((int)(p0 - p5));
        }
    }
}