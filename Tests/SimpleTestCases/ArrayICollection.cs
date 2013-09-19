using System;
using System.Collections.Generic;

static class Program {
    public static void Main () {
        int[] arr = new int[] { 2, 3, 5, 7, 11 };
        ICollection<int> col = arr;
        Console.WriteLine("Count: {0}", col.Count);
    }
}
