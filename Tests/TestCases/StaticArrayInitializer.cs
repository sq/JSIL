using System;

public static class Program {
    public static void Main (string[] args) {
        int[][,] array = new int[][,] {
            new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
            new int[,] { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} },
            new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
            new int[,] { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} }
        };

        for (var z = 0; z < array.Length; z++) {
            Console.WriteLine("-- {0} --", z);

            var subarray = array[z];
            for (int y = 0, h = subarray.GetLength(0); y < h; y++) {
                for (int x = 0, w = subarray.GetLength(1); x < w; x++) {
                    Console.WriteLine("[{0}, {1}] = {2}", y, x, subarray[y, x]);
                }
            }
        }
    }
}